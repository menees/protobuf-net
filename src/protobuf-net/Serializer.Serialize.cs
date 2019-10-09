﻿using ProtoBuf.Internal;
using ProtoBuf.Meta;
using System;
using System.Buffers;
using System.IO;

namespace ProtoBuf
{
    partial class Serializer
    {

        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream.
        /// </summary>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="destination">The destination stream to write to.</param>
        public static void Serialize<T>(Stream destination, T instance)
        {
            var state = ProtoWriter.State.Create(destination, RuntimeTypeModel.Default);
            try
            {
                TypeModel.SerializeImpl<T>(ref state, instance);
            }
            finally
            {
                state.Dispose();
            }
        }


        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied writer.
        /// </summary>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="destination">The destination stream to write to.</param>
        /// <param name="context">Additional serialization context</param>
        public static void Serialize<T>(IBufferWriter<byte> destination, T instance, SerializationContext context = null)
        {
            var state = ProtoWriter.State.Create(destination, RuntimeTypeModel.Default, context);
            try
            {
                TypeModel.SerializeImpl<T>(ref state, instance);
            }
            finally
            {
                state.Dispose();
            }
        }

        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied SerializationInfo.
        /// </summary>
        /// <typeparam name="T">The type being serialized.</typeparam>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="info">The destination SerializationInfo to write to.</param>
        public static void Serialize<T>(System.Runtime.Serialization.SerializationInfo info, T instance) where T : class, System.Runtime.Serialization.ISerializable
        {
            Serialize<T>(info, new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.Persistence), instance);
        }
        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied SerializationInfo.
        /// </summary>
        /// <typeparam name="T">The type being serialized.</typeparam>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="info">The destination SerializationInfo to write to.</param>
        /// <param name="context">Additional information about this serialization operation.</param>
        public static void Serialize<T>(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context, T instance) where T : class, System.Runtime.Serialization.ISerializable
        {
            // note: also tried byte[]... it doesn't perform hugely well with either (compared to regular serialization)
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.GetType() != typeof(T)) throw new ArgumentException("Incorrect type", nameof(instance));
            using MemoryStream ms = new MemoryStream();
            RuntimeTypeModel.Default.Serialize(ms, instance, context);
            info.AddValue(ProtoBinaryField, ms.ToArray());
        }

        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied XmlWriter.
        /// </summary>
        /// <typeparam name="T">The type being serialized.</typeparam>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="writer">The destination XmlWriter to write to.</param>
        public static void Serialize<T>(System.Xml.XmlWriter writer, T instance) where T : System.Xml.Serialization.IXmlSerializable
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
#pragma warning disable RCS1165 // Unconstrained type parameter checked for null.
            if (instance == null) throw new ArgumentNullException(nameof(instance));
#pragma warning restore RCS1165 // Unconstrained type parameter checked for null.

            using MemoryStream ms = new MemoryStream();
            Serializer.Serialize<T>(ms, instance);
            Helpers.GetBuffer(ms, out var segment);
            writer.WriteBase64(segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream,
        /// with a length-prefix. This is useful for socket programming,
        /// as DeserializeWithLengthPrefix/MergeWithLengthPrefix can be used to read the single object back
        /// from an ongoing stream.
        /// </summary>
        /// <typeparam name="T">The type being serialized.</typeparam>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="destination">The destination stream to write to.</param>
        public static void SerializeWithLengthPrefix<T>(Stream destination, T instance, PrefixStyle style)
        {
            SerializeWithLengthPrefix<T>(destination, instance, style, 0);
        }

        /// <summary>
        /// Writes a protocol-buffer representation of the given instance to the supplied stream,
        /// with a length-prefix. This is useful for socket programming,
        /// as DeserializeWithLengthPrefix/MergeWithLengthPrefix can be used to read the single object back
        /// from an ongoing stream.
        /// </summary>
        /// <typeparam name="T">The type being serialized.</typeparam>
        /// <param name="instance">The existing instance to be serialized (cannot be null).</param>
        /// <param name="style">How to encode the length prefix.</param>
        /// <param name="destination">The destination stream to write to.</param>
        /// <param name="fieldNumber">The tag used as a prefix to each record (only used with base-128 style prefixes).</param>
        public static void SerializeWithLengthPrefix<T>(Stream destination, T instance, PrefixStyle style, int fieldNumber)
        {
            RuntimeTypeModel.Default.SerializeWithLengthPrefix(destination, instance, typeof(T), style, fieldNumber);
        }
    }
}