using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [Timer(TimerType.UnitTransferTimeout)]
    public class UnitTransferTimeout : ATimer<Tuple<UnitComponent, long, long>>
    {
        public override void Run(Tuple<UnitComponent, long, long> args)
        {
            UnitComponent owner = args.Item1;
            if (owner.IsDisposed || !owner.UnitComponents.TryGetValue(args.Item2, out UnitTransferBuffer buffer)
                || buffer.TransferId != args.Item3)
            {
                return;
            }

            if (buffer.ExpireTime > TimeHelper.ServerNow())
            {
                TimerComponent.Instance.NewOnceTimer(buffer.ExpireTime, TimerType.UnitTransferTimeout, args);
                return;
            }

            owner.UnitComponents.Remove(args.Item2);
        }
    }

    public static class UnitTransferHelper
    {
        public const int ChunkSize = 256 * 1024;
        public const int MaxTransferBytes = 64 * 1024 * 1024;
        public const long Timeout = 60 * 1000;

        // 每个实体单独编码长度，单个大组件也可以跨越多个网络分片。
        public static byte[] Serialize(Unit unit)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            if (!WriteEntity(writer, unit))
            {
                return null;
            }

            foreach (Entity entity in unit.Components.Values)
            {
                if (entity is ITransfer && !WriteEntity(writer, entity))
                {
                    return null;
                }
            }

            return stream.ToArray();
        }

        private static bool WriteEntity(BinaryWriter writer, Entity entity)
        {
            byte[] bytes = MongoHelper.ToBson(entity);
            if (writer.BaseStream.Length + sizeof(int) + bytes.Length > MaxTransferBytes)
            {
                return false;
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
            return true;
        }

        public static async ETTask<int> Send(long unitId, long sceneInstanceId, long transferId, byte[] bytes)
        {
            int chunkCount = (bytes.Length + ChunkSize - 1) / ChunkSize;
            for (int index = 0; index < chunkCount; index++)
            {
                int offset = index * ChunkSize;
                byte[] data = new byte[Math.Min(ChunkSize, bytes.Length - offset)];
                Buffer.BlockCopy(bytes, offset, data, 0, data.Length);
                M2M_UnitTransfer_0_Response response = (M2M_UnitTransfer_0_Response)await ActorMessageSenderComponent.Instance.Call(
                    sceneInstanceId, new M2M_UnitTransfer_0_Request
                    {
                        UnitId = unitId,
                        TransferId = transferId,
                        ChunkIndex = index,
                        ChunkCount = chunkCount,
                        TotalBytes = bytes.Length,
                        Data = data,
                    });
                if (response.Error != ErrorCode.ERR_Success)
                {
                    return response.Error;
                }
            }

            return ErrorCode.ERR_Success;
        }

        public static int Receive(UnitComponent owner, M2M_UnitTransfer_0_Request request, out bool started)
        {
            started = false;
            if (request.UnitId == 0 || request.TransferId == 0 || request.TotalBytes <= 0 || request.TotalBytes > MaxTransferBytes
                || request.ChunkCount != (request.TotalBytes + ChunkSize - 1) / ChunkSize
                || request.ChunkIndex < 0 || request.ChunkIndex >= request.ChunkCount)
            {
                return ErrorCode.ERR_ModifyData;
            }

            int expectedBytes = Math.Min(ChunkSize, request.TotalBytes - request.ChunkIndex * ChunkSize);
            if (request.Data == null || request.Data.Length != expectedBytes)
            {
                return ErrorCode.ERR_ModifyData;
            }

            long now = TimeHelper.ServerNow();
            owner.UnitComponents.TryGetValue(request.UnitId, out UnitTransferBuffer buffer);
            if (buffer != null && buffer.ExpireTime <= now)
            {
                owner.UnitComponents.Remove(request.UnitId);
                buffer = null;
            }

            if (buffer == null || buffer.TransferId != request.TransferId)
            {
                if (request.ChunkIndex != 0)
                {
                    return ErrorCode.ERR_TransferFailError;
                }

                buffer = new UnitTransferBuffer
                {
                    TransferId = request.TransferId,
                    ChunkCount = request.ChunkCount,
                    TotalBytes = request.TotalBytes,
                };
                owner.UnitComponents[request.UnitId] = buffer;
                started = true;
            }

            if (buffer.ChunkCount != request.ChunkCount || buffer.TotalBytes != request.TotalBytes
                || request.ChunkIndex > buffer.Chunks.Count)
            {
                return ErrorCode.ERR_TransferFailError;
            }

            if (request.ChunkIndex < buffer.Chunks.Count)
            {
                return request.Data.AsSpan().SequenceEqual(buffer.Chunks[request.ChunkIndex])
                        ? ErrorCode.ERR_Success : ErrorCode.ERR_TransferFailError;
            }

            buffer.Chunks.Add(request.Data);
            buffer.ReceivedBytes += request.Data.Length;
            buffer.ExpireTime = now + Timeout;
            return ErrorCode.ERR_Success;
        }

        public static bool TryTake(UnitComponent owner, long unitId, long transferId, out Unit unit, out List<Entity> entities)
        {
            unit = null;
            entities = new List<Entity>();
            if (!owner.UnitComponents.TryGetValue(unitId, out UnitTransferBuffer buffer) || buffer.TransferId != transferId
                || buffer.Chunks.Count != buffer.ChunkCount || buffer.ReceivedBytes != buffer.TotalBytes
                || buffer.ExpireTime <= TimeHelper.ServerNow())
            {
                return false;
            }

            owner.UnitComponents.Remove(unitId);
            using MemoryStream stream = new MemoryStream(buffer.TotalBytes);
            foreach (byte[] chunk in buffer.Chunks)
            {
                stream.Write(chunk, 0, chunk.Length);
            }

            stream.Position = 0;
            using BinaryReader reader = new BinaryReader(stream);
            if (!TryReadEntity(reader, out byte[] unitBytes))
            {
                return false;
            }

            unit = MongoHelper.Deserialize<Unit>(unitBytes);
            if (unit.Id != unitId)
            {
                return false;
            }

            HashSet<Type> types = new HashSet<Type>();
            while (stream.Position < stream.Length)
            {
                if (!TryReadEntity(reader, out byte[] bytes))
                {
                    return false;
                }

                Entity entity = MongoHelper.Deserialize<Entity>(bytes);
                if (!(entity is ITransfer) || !types.Add(entity.GetType()))
                {
                    return false;
                }

                entities.Add(entity);
            }

            return true;
        }

        private static bool TryReadEntity(BinaryReader reader, out byte[] bytes)
        {
            bytes = null;
            if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(int))
            {
                return false;
            }

            int length = reader.ReadInt32();
            if (length < 5 || length > reader.BaseStream.Length - reader.BaseStream.Position)
            {
                return false;
            }

            bytes = reader.ReadBytes(length);
            return true;
        }
    }
}
