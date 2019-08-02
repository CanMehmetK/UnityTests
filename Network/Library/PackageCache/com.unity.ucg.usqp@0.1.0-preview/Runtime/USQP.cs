using System;
using System.Text;
using Unity.Networking.Transport;
using UnityEngine;

namespace Unity.Ucg.Usqp
{
    [Flags]
    public enum UsqpChunkType
    {
        ServerInfo = 1,
        ServerRules = 2,
        PlayerInfo = 4,
        TeamInfo = 8
    }

    public enum UsqpMessageType
    {
        ChallengeRequest = 0,
        ChallengeResponse = 0,
        QueryRequest = 1,
        QueryResponse = 1
    }

    public interface IUsqpMessage
    {
        void ToStream(ref DataStreamWriter writer);
        void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx);
    }

    public struct UsqpHeader : IUsqpMessage
    {
        public byte Type { get; internal set; }
        public uint ChallengeId;

        public void ToStream(ref DataStreamWriter writer)
        {
            writer.Write(Type);
            writer.WriteNetworkByteOrder(ChallengeId);
        }

        public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            Type = reader.ReadByte(ref ctx);
            ChallengeId = reader.ReadUIntNetworkByteOrder(ref ctx);
        }
    }

    public struct ChallengeRequest : IUsqpMessage
    {
        public UsqpHeader Header;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.ChallengeRequest;
            Header.ToStream(ref writer);
        }

        public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            Header.FromStream(reader, ref ctx);
        }
    }

    public struct ChallengeResponse
    {
        public UsqpHeader Header;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.ChallengeResponse;
            Header.ToStream(ref writer);
        }

        public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            Header.FromStream(reader, ref ctx);
        }
    }

    public struct QueryRequest
    {
        public UsqpHeader Header;
        public ushort Version;

        public byte RequestedChunks;

        public void ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.QueryRequest;

            Header.ToStream(ref writer);
            writer.WriteNetworkByteOrder(Version);
            writer.Write(RequestedChunks);
        }

        public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            Header.FromStream(reader, ref ctx);
            Version = reader.ReadUShortNetworkByteOrder(ref ctx);
            RequestedChunks = reader.ReadByte(ref ctx);
        }
    }

    public struct QueryResponseHeader
    {
        public UsqpHeader Header;
        public ushort Version;
        public byte CurrentPacket;
        public byte LastPacket;
        public ushort Length;

        public DataStreamWriter.DeferredUShortNetworkByteOrder ToStream(ref DataStreamWriter writer)
        {
            Header.Type = (byte)UsqpMessageType.QueryResponse;
            Header.ToStream(ref writer);
            writer.WriteNetworkByteOrder(Version);
            writer.Write(CurrentPacket);
            writer.Write(LastPacket);
            return writer.WriteNetworkByteOrder(Length);
        }

        public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            Header.FromStream(reader, ref ctx);
            Version = reader.ReadUShortNetworkByteOrder(ref ctx);
            CurrentPacket = reader.ReadByte(ref ctx);
            LastPacket = reader.ReadByte(ref ctx);
            Length = reader.ReadUShortNetworkByteOrder(ref ctx);
        }
    }

    public class ServerInfo
    {
        static Encoding s_Encoding = new UTF8Encoding();
        static Encoder s_Encoder;

        public uint ChunkLen;
        public QueryResponseHeader QueryHeader;
        public Data ServerInfoData;

        public ServerInfo()
        {
            ServerInfoData = new Data();
        }

        public void ToStream(ref DataStreamWriter writer)
        {
            var lengthValue = QueryHeader.ToStream(ref writer);

            var start = (ushort)writer.Length;

            var chunkValue = writer.WriteNetworkByteOrder((uint)0); // ChunkLen

            var chunkStart = (uint)writer.Length;
            ServerInfoData.ToStream(ref writer);
            ChunkLen = (uint)writer.Length - chunkStart;
            QueryHeader.Length = (ushort)(writer.Length - start);

            lengthValue.Update(QueryHeader.Length);
            chunkValue.Update(ChunkLen);
        }

        public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
        {
            QueryHeader.FromStream(reader, ref ctx);
            ChunkLen = reader.ReadUIntNetworkByteOrder(ref ctx);

            ServerInfoData.FromStream(reader, ref ctx);
        }

        [Serializable]
        public class Data
        {
            public string BuildId = "";
            public ushort CurrentPlayers;
            public string GameType = "";
            public string Map = "";
            public ushort MaxPlayers;
            public ushort Port;
            public string ServerName = "";

            static unsafe void WriteString(DataStreamWriter writer, string value)
            {
                s_Encoder = s_Encoder ?? s_Encoding.GetEncoder();
                var buffer = new byte[byte.MaxValue];
                var chars = value.ToCharArray();

                s_Encoder.Convert(chars, 0, chars.Length, buffer, 0, byte.MaxValue, true, out var charsUsed, out var bytesUsed, out var completed);
                Debug.Assert(bytesUsed <= byte.MaxValue);

                writer.Write((byte)bytesUsed);

                fixed (byte* bufferPtr = &buffer[0])
                {
                    writer.WriteBytes(bufferPtr, bytesUsed);
                }
            }

            static unsafe string ReadString(DataStreamReader reader, ref DataStreamReader.Context ctx)
            {
                var length = reader.ReadByte(ref ctx);
                var buffer = new byte[byte.MaxValue];

                fixed (byte* bufferPtr = &buffer[0])
                {
                    reader.ReadBytes(ref ctx, bufferPtr, length);
                }

                return s_Encoding.GetString(buffer, 0, length);
            }

            public void ToStream(ref DataStreamWriter writer)
            {
                writer.WriteNetworkByteOrder(CurrentPlayers);
                writer.WriteNetworkByteOrder(MaxPlayers);

                WriteString(writer, ServerName);
                WriteString(writer, GameType);
                WriteString(writer, BuildId);
                WriteString(writer, Map);

                writer.WriteNetworkByteOrder(Port);
            }

            public void FromStream(DataStreamReader reader, ref DataStreamReader.Context ctx)
            {
                CurrentPlayers = reader.ReadUShortNetworkByteOrder(ref ctx);
                MaxPlayers = reader.ReadUShortNetworkByteOrder(ref ctx);

                ServerName = ReadString(reader, ref ctx);
                GameType = ReadString(reader, ref ctx);
                BuildId = ReadString(reader, ref ctx);
                Map = ReadString(reader, ref ctx);

                Port = reader.ReadUShortNetworkByteOrder(ref ctx);
            }
        }
    }
}
