/**
 * \cond
 */
using System;
using System.Globalization;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SpringCard.LibCs
{
	
	public class JSON
	{
		private struct Entry
		{
			public string Name;
			public object Value;
			public JSONObjectType Type;
		}
		
		private List<Entry> entries = new List<Entry>();
		
		public JSON()
		{
			 
		}

		public void Add(string Name, string Value)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = Value;
			e.Type = JSONObjectType.String;
			entries.Add(e);
		}

		public void Add(string Name, bool Value)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = Value;
			e.Type = JSONObjectType.Boolean;
			entries.Add(e);
		}

		public void Add(string Name, double Value)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = Value.ToString();
			e.Type = JSONObjectType.Number;
			entries.Add(e);
		}
		
		public void Add(string Name, ulong Value)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = Value.ToString();
			e.Type = JSONObjectType.Number;
			entries.Add(e);
		}

		public void Add(string Name, long Value)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = Value.ToString();
			e.Type = JSONObjectType.Number;
			entries.Add(e);
		}
		
		public void Add(string Name, JSON Value)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = Value;
			e.Type = JSONObjectType.Object;
			entries.Add(e);
		}
		
		public void Add(string Name)
		{
			Entry e = new Entry();
			e.Name = Name;
			e.Value = null;
			e.Type = JSONObjectType.Null;
			entries.Add(e);
		}		
		
		public string AsString()
		{			
			string result = "{";
			
			for (int i=0; i<entries.Count; i++)
			{				
				Entry entry = entries[i];
				
				if (i>0)
					result += ",";
								
				result += "\"" + entry.Name + "\":";
				
				switch (entry.Type)
				{
					case JSONObjectType.Object :
						result += (entry.Value as JSON).AsString();
						break;
					case JSONObjectType.Array :
						result += "\"\"";
						break;
					case JSONObjectType.String :
						result += "\"";
						result += (string) entry.Value;
						result += "\"";
						break;						
					case JSONObjectType.Number :
						result += (string) entry.Value;
						break;
					case JSONObjectType.Boolean :
						{
							bool value = (bool) entry.Value;
							result += value ? "true" : "false";
						}
						break;						
					case JSONObjectType.Null :
					default :
						result += "null";
						break;
				}
			}
			
			result += "}";
			return result;
		}
	}
	
	/*
	 * SimpleJSON for .NET 
	 * 
	 * Copyright (c) 2011, Boldai AB All rights reserved.
	 * 
	 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
	 * 
	 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
	 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
	 * Neither the name of the <organization> nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
	 * 
	 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE
	 * 
	 */
	
	public enum JSONObjectType
	{
		Object,
		Array,
		String,
		Number,
		Boolean,
		Null
	}

	public enum IntegerSize
	{
		UInt64,
		Int64,
		UInt32,
		Int32,
		UInt16,
		Int16,
		UInt8,
		Int8,
	}

	public enum FloatSize
	{
		Double,
		Single
	}

	public class JSONObject
	{
		public JSONObjectType Kind { get; private set; }

		public Dictionary<string, JSONObject> ObjectValue { get; private set; }
		public List<JSONObject> ArrayValue { get; private set; }
		public string StringValue { get; private set; }
		public bool BooleanValue { get; private set; }

		public int Count {
			get {
				return Kind == JSONObjectType.Array ? ArrayValue.Count
                     : Kind == JSONObjectType.Object ? ObjectValue.Count
                     : 0;
			}
		}

		public double DoubleValue { get; private set; }
		public float FloatValue { get; private set; }
		public ulong ULongValue { get; private set; }
		public long LongValue { get; private set; }
		public uint UIntValue { get; private set; }
		public int IntValue { get; private set; }
		public ushort UShortValue { get; private set; }
		public short ShortValue { get; private set; }
		public byte ByteValue { get; private set; }
		public sbyte SByteValue { get; private set; }

		public bool IsNegative { get; private set; }
		public bool IsFractional { get; private set; }
		public IntegerSize MinInteger { get; private set; }
		public FloatSize MinFloat { get; private set; }

		public JSONObject this[string key] { get { return ObjectValue[key]; } }
		public JSONObject this[int key] { get { return ArrayValue[key]; } }

		public static explicit operator string(JSONObject obj)
		{
			return obj.StringValue;
		}
		public static explicit operator bool(JSONObject obj)
		{
			return obj.BooleanValue;
		}
		public static explicit operator double(JSONObject obj)
		{
			return obj.DoubleValue;
		}
		public static explicit operator float(JSONObject obj)
		{
			return obj.FloatValue;
		}
		public static explicit operator ulong(JSONObject obj)
		{
			return obj.ULongValue;
		}
		public static explicit operator long(JSONObject obj)
		{
			return obj.LongValue;
		}
		public static explicit operator uint(JSONObject obj)
		{
			return obj.UIntValue;
		}
		public static explicit operator int(JSONObject obj)
		{
			return obj.IntValue;
		}
		public static explicit operator ushort(JSONObject obj)
		{
			return obj.UShortValue;
		}
		public static explicit operator short(JSONObject obj)
		{
			return obj.ShortValue;
		}
		public static explicit operator byte(JSONObject obj)
		{
			return obj.ByteValue;
		}
		public static explicit operator sbyte(JSONObject obj)
		{
			return obj.SByteValue;
		}

		public static JSONObject CreateString(string str)
		{
			return new JSONObject(str);
		}
		public static JSONObject CreateBoolean(bool b)
		{
			return new JSONObject(b);
		}
		public static JSONObject CreateNull()
		{
			return new JSONObject();
		}
		public static JSONObject CreateNumber(bool isNegative, bool isFractional, bool negativeExponent, ulong integerPart, ulong fractionalPart, int fractionalPartLength, ulong exponent)
		{
			return new JSONObject(isNegative, isFractional, negativeExponent, integerPart, fractionalPart, fractionalPartLength, exponent);
		}
		public static JSONObject CreateArray(List<JSONObject> list)
		{
			return new JSONObject(list);
		}
        public static JSONObject CreateArray()
        {
            return new JSONObject(new List<JSONObject>());
        }
        public static JSONObject CreateObject(Dictionary<string, JSONObject> dict)
		{
			return new JSONObject(dict);
		}
        public static JSONObject CreateObject()
        {
            return new JSONObject(new Dictionary<string, JSONObject>());
        }

        public override string ToString()
		{
			return JSONEncoder.Encode(this);
		}
        public string ToString(JSONEncoder.Format format)
        {
            return JSONEncoder.Encode(this, format);
        }

        private JSONObject(string str)
		{
			Kind = JSONObjectType.String;
			StringValue = str;
		}

		private JSONObject(bool b)
		{
			Kind = JSONObjectType.Boolean;
			BooleanValue = b;
		}

		private JSONObject()
		{
			Kind = JSONObjectType.Null;
		}

		private JSONObject(bool isNegative, bool isFractional, bool negativeExponent, ulong integerPart, ulong fractionalPart, int fractionalPartLength, ulong exponent)
		{
			Kind = JSONObjectType.Number;
			if (!isFractional) {
				MakeInteger(isNegative, integerPart);
			} else {
				MakeFloat(isNegative, negativeExponent, integerPart, fractionalPart, fractionalPartLength, exponent);
			}
		}

		private JSONObject(List<JSONObject> list)
		{
			Kind = JSONObjectType.Array;
			ArrayValue = list;
		}

		private JSONObject(Dictionary<string, JSONObject> dict)
		{
			Kind = JSONObjectType.Object;
			ObjectValue = dict;
		}

		private void MakeInteger(bool isNegative, ulong integerPart)
		{
			IsNegative = isNegative;

			if (!IsNegative) {
				ULongValue = integerPart;
				MinInteger = IntegerSize.UInt64;

				if (ULongValue <= Int64.MaxValue) {
					LongValue = (long)ULongValue;
					MinInteger = IntegerSize.Int64;
				}

				if (ULongValue <= UInt32.MaxValue) {
					UIntValue = (uint)ULongValue;
					MinInteger = IntegerSize.UInt32;
				}

				if (ULongValue <= Int32.MaxValue) {
					IntValue = (int)ULongValue;
					MinInteger = IntegerSize.Int32;
				}

				if (ULongValue <= UInt16.MaxValue) {
					UShortValue = (ushort)ULongValue;
					MinInteger = IntegerSize.UInt16;
				}

				if (ULongValue <= (ulong)Int16.MaxValue) {
					ShortValue = (short)ULongValue;
					MinInteger = IntegerSize.Int16;
				}

				if (ULongValue <= Byte.MaxValue) {
					ByteValue = (byte)ULongValue;
					MinInteger = IntegerSize.UInt8;
				}

				if (ULongValue <= (ulong)SByte.MaxValue) {
					SByteValue = (sbyte)ULongValue;
					MinInteger = IntegerSize.Int8;
				}

				DoubleValue = ULongValue;
				MinFloat = FloatSize.Double;

				if (DoubleValue <= Single.MaxValue) {
					FloatValue = (float)DoubleValue;
					MinFloat = FloatSize.Single;
				}
			} else {
				LongValue = -(long)integerPart;
				MinInteger = IntegerSize.Int64;

				if (LongValue >= Int32.MinValue) {
					IntValue = (int)LongValue;
					MinInteger = IntegerSize.Int32;
				}

				if (LongValue >= Int16.MinValue) {
					ShortValue = (short)LongValue;
					MinInteger = IntegerSize.Int16;
				}

				if (LongValue >= SByte.MinValue) {
					SByteValue = (sbyte)LongValue;
					MinInteger = IntegerSize.Int8;
				}

				DoubleValue = LongValue;
				MinFloat = FloatSize.Double;

				if (DoubleValue >= -Single.MaxValue) {
					FloatValue = (float)DoubleValue;
					MinFloat = FloatSize.Single;
				}
			}
		}

		private void MakeFloat(bool isNegative, bool negativeExponent, ulong integerPart, ulong fractionalPart, int fractionalPartLength, ulong exponent)
		{
			DoubleValue = (isNegative ? -1 : 1) * ((double)integerPart + (double)fractionalPart / Math.Pow(10, fractionalPartLength)) * Math.Pow(10, (negativeExponent ? -1 : 1) * (long)exponent);
			MinFloat = FloatSize.Double;
			IsFractional = true;

			if (DoubleValue < 0) {
				IsNegative = true;

				if (DoubleValue >= -Single.MaxValue) {
					FloatValue = (float)DoubleValue;
					MinFloat = FloatSize.Single;
				}
			} else if (DoubleValue <= Single.MaxValue) {
				FloatValue = (float)DoubleValue;
				MinFloat = FloatSize.Single;
			}
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, this))
				return true;
			if (!(obj is JSONObject))
				return false;

			var jobj = (JSONObject)obj;
			if (jobj.Kind != Kind)
				return false;

			switch (Kind) {
				case JSONObjectType.Array:
					if (ArrayValue.Count != jobj.ArrayValue.Count)
						return false;
					for (var i = 0; i < ArrayValue.Count; ++i) {
						if (!ArrayValue[i].Equals(jobj.ArrayValue[i]))
							return false;
					}
					return true;
				case JSONObjectType.Boolean:
					return BooleanValue == jobj.BooleanValue;
				case JSONObjectType.Number:
					return EqualNumber(this, jobj);
				case JSONObjectType.Object:
					if (ObjectValue.Count != jobj.ObjectValue.Count)
						return false;
					foreach (var pair in ObjectValue) {
						if (!jobj.ObjectValue.ContainsKey(pair.Key) ||
						                  !pair.Value.Equals(jobj.ObjectValue[pair.Key]))
							return false;
					}
					return true;
				case JSONObjectType.String:
					return StringValue == jobj.StringValue;
			}

			return true;
		}

		public override int GetHashCode()
		{
			switch (Kind) {
				case JSONObjectType.Array:
					return ArrayValue.GetHashCode();
				case JSONObjectType.Boolean:
					return BooleanValue.GetHashCode();
				case JSONObjectType.Null:
					return 0;
				case JSONObjectType.Object:
					return ObjectValue.GetHashCode();
				case JSONObjectType.String:
					return StringValue.GetHashCode();
				case JSONObjectType.Number:
					if (IsFractional)
						return DoubleValue.GetHashCode();
					if (IsNegative)
						return LongValue.GetHashCode();
					return ULongValue.GetHashCode();
			}
			return 0;
		}

		public static bool EqualNumber(JSONObject o1, JSONObject o2)
		{
			if (o1.MinFloat != o2.MinFloat ||
			             o1.MinInteger != o2.MinInteger ||
			             o1.IsNegative != o2.IsNegative ||
			             o1.IsFractional != o2.IsFractional)
				return false;

			if (o1.IsFractional) {
				return o1.DoubleValue == o2.DoubleValue;
			}
			if (o1.IsNegative) {
				return o1.LongValue == o2.LongValue;
			}

			return o1.ULongValue == o2.ULongValue;
		}
		
		public JSONObject Get(string Name)
		{
			try
			{ 
				return (JSONObject) this[Name];
			}
			catch (System.Collections.Generic.KeyNotFoundException)
			{
				return null;
			}			
		}
		
		public string GetString(string Name)
		{
			try
			{
				JSONObject o = this[Name];
				if (o == null) return "";
				return o.StringValue;
			}
			catch (System.Collections.Generic.KeyNotFoundException)
			{
				return "";
			}		
		}
		
		public UInt32 GetUInt32(string Name)
		{
			try
			{
				JSONObject o = this[Name];
				if (o == null) return 0;
				if (!String.IsNullOrEmpty(o.StringValue))
				{
					UInt32 r;
					UInt32.TryParse(o.StringValue, out r);
					return r;
				}
				return o.UIntValue;
			}
			catch (System.Collections.Generic.KeyNotFoundException)
			{
				return 0;
			}		
		}

        public int GetInteger(string Name)
        {
            try
            {
                JSONObject o = this[Name];
                if (o == null) return 0;
                if (!String.IsNullOrEmpty(o.StringValue))
                {
                    int r;
                    int.TryParse(o.StringValue, out r);
                    return r;
                }
                return o.IntValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return 0;
            }
        }

        public uint GetUnsigned(string Name)
        {
            try
            {
                JSONObject o = this[Name];
                if (o == null) return 0;
                if (!String.IsNullOrEmpty(o.StringValue))
                {
                    uint r;
                    uint.TryParse(o.StringValue, out r);
                    return r;
                }
                return o.UIntValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return 0;
            }
        }

        public bool GetBool(string Name)
		{
			try
			{
				JSONObject o = this[Name];
				if (o == null) return false;
				if (!String.IsNullOrEmpty(o.StringValue))
				{
					bool r;
					Boolean.TryParse(o.StringValue, out r);
					return r;
				}				
				return o.BooleanValue;
			}
			catch (System.Collections.Generic.KeyNotFoundException)
			{
				return false;
			}		
		}

        public void Add(string Name, JSONObject Value)
        {
            if (Kind != JSONObjectType.Object)
                throw new Exception("JSON is not an object");
            ObjectValue.Add(Name, Value);
        }

        public void Add(JSONObject Value)
        {
            if (Kind != JSONObjectType.Array)
                throw new Exception("JSON is not an array");
            ArrayValue.Add(Value);
        }
    }

    public class JSONEncoder
	{
        public enum Format
        {
            None,
            Indented
        }

        public static string Encode(object obj)
		{
			var encoder = new JSONEncoder();
			encoder.EncodeObject(obj);
			return encoder._buffer.ToString();
		}

        public static string Encode(object obj, Format format)
        {
            var encoder = new JSONEncoder(format);
            encoder.EncodeObject(obj);
            return encoder._buffer.ToString();
        }

        private readonly StringBuilder _buffer = new StringBuilder();
        private Format _format = Format.None;
        private int _indent = 0;
        private bool _newline = true;

        internal static readonly Dictionary<char, string> EscapeChars =
			new Dictionary<char, string> {
				{ '"', "\\\"" },
				{ '\\', "\\\\" },
				{ '\b', "\\b" },
				{ '\f', "\\f" },
				{ '\n', "\\n" },
				{ '\r', "\\r" },
				{ '\t', "\\t" },
				{ '\u2028', "\\u2028" },
				{ '\u2029', "\\u2029" }
			};

		private JSONEncoder()
		{
            
		}

        private JSONEncoder(Format format)
        {
            _format = format;
        }

        private void Indent()
        {
            if (_format == Format.Indented)
            {
                if (!_newline)
                {
                    _buffer.Append(Environment.NewLine);
                    _newline = true;
                }
                for (int i = 0; i < _indent; i++)
                    _buffer.Append("\t");
                _newline = false;
            }
        }

        private void IndentBegin()
        {
            Indent();
            _indent++;
        }

        private void IndentEnd()
        {
            _indent--;
            Indent();
        }

        private void EncodeObject(object obj)
		{
			if (obj == null) {
				EncodeNull();
			} else if (obj is string) {
				EncodeString((string)obj);
			} else if (obj is float) {
				EncodeFloat((float)obj);
			} else if (obj is double) {
				EncodeDouble((double)obj);
			} else if (obj is int) {
				EncodeLong((int)obj);
			} else if (obj is uint) {
				EncodeULong((uint)obj);
			} else if (obj is long) {
				EncodeLong((long)obj);
			} else if (obj is ulong) {
				EncodeULong((ulong)obj);
			} else if (obj is short) {
				EncodeLong((short)obj);
			} else if (obj is ushort) {
				EncodeULong((ushort)obj);
			} else if (obj is byte) {
				EncodeULong((byte)obj);
			} else if (obj is bool) {
				EncodeBool((bool)obj);
			} else if (obj is IDictionary) {
				EncodeDictionary((IDictionary)obj);
			} else if (obj is IEnumerable) {
				EncodeEnumerable((IEnumerable)obj);
			} else if (obj is Enum) {
				EncodeObject(Convert.ChangeType(obj, Enum.GetUnderlyingType(obj.GetType())));
			} else if (obj is JSONObject) {
				var jobj = (JSONObject)obj;
				switch (jobj.Kind) {
					case JSONObjectType.Array:
						EncodeEnumerable(jobj.ArrayValue);
						break;
					case JSONObjectType.Boolean:
						EncodeBool(jobj.BooleanValue);
						break;
					case JSONObjectType.Null:
						EncodeNull();
						break;
					case JSONObjectType.Number:
						if (jobj.IsFractional) {
							EncodeDouble(jobj.DoubleValue);
						} else if (jobj.IsNegative) {
							EncodeLong(jobj.LongValue);
						} else {
							EncodeULong(jobj.ULongValue);
						}
						break;
					case JSONObjectType.Object:
						EncodeDictionary(jobj.ObjectValue);
						break;
					case JSONObjectType.String:
						EncodeString(jobj.StringValue);
						break;
					default:
						throw new ArgumentException("Can't serialize object of type " + obj.GetType().Name, "obj");
				}
			} else {
				throw new ArgumentException("Can't serialize object of type " + obj.GetType().Name, "obj");
			}
		}

		private void EncodeNull()
		{
			_buffer.Append("null");
		}

		private void EncodeString(string str)
		{
			_buffer.Append('"');
			foreach (var c in str) {
				if (EscapeChars.ContainsKey(c)) {
					_buffer.Append(EscapeChars[c]);
				} else {
					if (c > 0x80 || c < 0x20) {
						_buffer.Append("\\u" + Convert.ToString(c, 16)
                                                   .ToUpper(CultureInfo.InvariantCulture)
                                                   .PadLeft(4, '0'));
					} else {
						_buffer.Append(c);
					}
				}
			}
			_buffer.Append('"');
		}

		private void EncodeFloat(float f)
		{
			_buffer.Append(f.ToString(CultureInfo.InvariantCulture));
		}

		private void EncodeDouble(double d)
		{
			_buffer.Append(d.ToString(CultureInfo.InvariantCulture));
		}

		private void EncodeLong(long l)
		{
			_buffer.Append(l.ToString(CultureInfo.InvariantCulture));
		}

		private void EncodeULong(ulong l)
		{
			_buffer.Append(l.ToString(CultureInfo.InvariantCulture));
		}

		private void EncodeBool(bool b)
		{
			_buffer.Append(b ? "true" : "false");
		}

		private void EncodeDictionary(IDictionary d)
		{
			var isFirst = true;
            IndentBegin();
            _buffer.Append('{');
            foreach (DictionaryEntry pair in d) {
				if (!(pair.Key is string)) {
					throw new ArgumentException("Dictionary keys must be strings", "d");
				}
                if (!isFirst)
                    _buffer.Append(',');
                Indent();
				EncodeString((string)pair.Key);
				_buffer.Append(':');
				EncodeObject(pair.Value);
				isFirst = false;
            }
            IndentEnd();
            _buffer.Append('}');
        }

		private void EncodeEnumerable(IEnumerable e)
		{
			var isFirst = true;
            IndentBegin();
            _buffer.Append('[');
            foreach (var obj in e) {
                if (!isFirst)
                    _buffer.Append(',');
                EncodeObject(obj);
				isFirst = false;
            }
            IndentEnd();
            _buffer.Append(']');
		}
	}

	public class JSONStreamEncoder
	{
		private struct EncoderContext
		{
			public bool IsObject;
			public bool IsEmpty;

			public EncoderContext(bool isObject, bool isEmpty)
			{
				IsObject = isObject;
				IsEmpty = isEmpty;
			}
		}

		private TextWriter _writer;
		private EncoderContext[] _contextStack;
		private int _contextStackPointer = -1;
		private bool _newlineInserted = false;

        public JSONStreamEncoder(TextWriter writer, int expectedNesting = 20)
		{
			_writer = writer;
			_contextStack = new EncoderContext[expectedNesting];
		}

		public void BeginArray()
		{
			WriteSeparator();
			PushContext(new EncoderContext(false, true));
			_writer.Write('[');
		}

		public void EndArray()
		{
			if (_contextStackPointer == -1) {
				throw new InvalidOperationException("EndArray called without BeginArray");
			} else if (_contextStack[_contextStackPointer].IsObject) {
				throw new InvalidOperationException("EndArray called after BeginObject");
			}

			PopContext();
			WriteNewline();
			_writer.Write(']');
		}

		public void BeginObject()
		{
			WriteSeparator();
			PushContext(new EncoderContext(true, true));
			_writer.Write('{');
		}

		public void EndObject()
		{
			if (_contextStackPointer == -1) {
				throw new InvalidOperationException("EndObject called without BeginObject");
			} else if (!_contextStack[_contextStackPointer].IsObject) {
				throw new InvalidOperationException("EndObject called after BeginArray");
			}

			PopContext();
			WriteNewline();
			_writer.Write('}');
		}

		public void WriteString(string str)
		{
			WriteSeparator();
			WriteBareString(str);
		}

		public void WriteKey(string str)
		{
			if (_contextStackPointer == -1) {
				throw new InvalidOperationException("WriteKey called without BeginObject");
			} else if (!_contextStack[_contextStackPointer].IsObject) {
				throw new InvalidOperationException("WriteKey called after BeginArray");
			}

			WriteSeparator();
			WriteBareString(str);
			_writer.Write(':');

			_contextStack[_contextStackPointer].IsEmpty = true;
		}

		public void WriteNumber(long l)
		{
			WriteSeparator();
			_writer.Write(l);
		}

		public void WriteNumber(ulong l)
		{
			WriteSeparator();
			_writer.Write(l);
		}

		public void WriteNumber(double d)
		{
			WriteSeparator();
			WriteFractionalNumber(d, 0.00000000000000001);
		}

		public void WriteNumber(float f)
		{
			WriteSeparator();
			WriteFractionalNumber(f, 0.000000001);
		}

		public void WriteNull()
		{
			WriteSeparator();
			_writer.Write("null");
		}

		public void WriteBool(bool b)
		{
			WriteSeparator();
			_writer.Write(b ? "true" : "false");
		}

		public void WriteJObject(JSONObject obj)
		{
			switch (obj.Kind) {
				case JSONObjectType.Array:
					BeginArray();
					foreach (var elem in obj.ArrayValue) {
						WriteJObject(elem);
					}
					EndArray();
					break;
				case JSONObjectType.Boolean:
					WriteBool(obj.BooleanValue);
					break;
				case JSONObjectType.Null:
					WriteNull();
					break;
				case JSONObjectType.Number:
					if (obj.IsFractional) {
						WriteNumber(obj.DoubleValue);
					} else if (obj.IsNegative) {
						WriteNumber(obj.LongValue);
					} else {
						WriteNumber(obj.ULongValue);
					}
					break;
				case JSONObjectType.Object:
					BeginObject();
					foreach (var pair in obj.ObjectValue) {
						WriteKey(pair.Key);
						WriteJObject(pair.Value);
					}
					EndObject();
					break;
				case JSONObjectType.String:
					WriteString(obj.StringValue);
					break;
			}
		}

		public void InsertNewline()
		{
			_newlineInserted = true;
		}

		private void WriteBareString(string str)
		{
			_writer.Write('"');

			int len = str.Length;
			int lastIndex = 0;
			int i = 0;

			for (; i < len; ++i) {
				char c = str[i];

				if (c > 0x80 || c < 0x20 || c == '"' || c == '\\') {
					if (i > lastIndex) {
						_writer.Write(str.Substring(lastIndex, i - lastIndex));
					}
					if (JSONEncoder.EscapeChars.ContainsKey(c)) {
						_writer.Write(JSONEncoder.EscapeChars[c]);
					} else {
						_writer.Write("\\u" + Convert.ToString(c, 16)
                                                .ToUpper(CultureInfo.InvariantCulture)
                                                .PadLeft(4, '0'));
					}
					lastIndex = i + 1;
				}
			}

			if (lastIndex == 0 && i > lastIndex) {
				_writer.Write(str);
			} else if (i > lastIndex) {
				_writer.Write(str.Substring(lastIndex, i - lastIndex));
			}

			_writer.Write('"');
		}

		private void WriteFractionalNumber(double d, double tolerance)
		{
			if (d < 0) {
				_writer.Write('-');
				d = -d;
			} else if (d == 0) {
				_writer.Write('0');
				return;
			}

			var magnitude = (int)Math.Log10(d);

			if (magnitude < 0) {
				_writer.Write("0.");
				for (int i = 0; i > magnitude + 1; --i) {
					_writer.Write('0');
				}
			}

			while (d > tolerance || magnitude >= 0) {
				var weight = Math.Pow(10, magnitude);
				var digit = (int)Math.Floor(d / weight);
				d -= digit * weight;
				_writer.Write((char)('0' + digit));
				if (magnitude == 0 && (d > tolerance || magnitude > 0)) {
					_writer.Write('.');
				}
				--magnitude;
			}
		}

		private void WriteSeparator()
		{
			if (_contextStackPointer == -1)
				return;

			if (!_contextStack[_contextStackPointer].IsEmpty) {
				_writer.Write(',');
			}

			_contextStack[_contextStackPointer].IsEmpty = false;

			WriteNewline();
		}

		private void WriteNewline()
		{
			if (_newlineInserted) {
				_writer.Write('\n');
				for (var i = 0; i < _contextStackPointer + 1; ++i) {
					_writer.Write(' ');
				}

				_newlineInserted = false;
			}
		}

		private void PushContext(EncoderContext ctx)
		{
			if (_contextStackPointer + 1 == _contextStack.Length) {
				throw new StackOverflowException("Too much nesting for context stack, increase expected nesting when creating the encoder");
			}

			_contextStack[++_contextStackPointer] = ctx;
		}

		private void PopContext()
		{
			if (_contextStackPointer == -1) {
				throw new InvalidOperationException("Stack underflow");
			}

			--_contextStackPointer;
		}
	}

	public class ParseError : Exception
	{
		public readonly int Position;

		public ParseError(string message, int position)
			: base(message)
		{
			Position = position;
		}
	}

	public static class JSONDecoder
	{
		private const char ObjectStart = '{';
		private const char ObjectEnd = '}';
		private const char ObjectPairSeparator = ',';
		private const char ObjectSeparator = ':';
		private const char ArrayStart = '[';
		private const char ArrayEnd = ']';
		private const char ArraySeparator = ',';
		private const char StringStart = '"';
		private const char NullStart = 'n';
		private const char TrueStart = 't';
		private const char FalseStart = 'f';

		public static JSONObject Decode(string json)
		{
			var data = Scan(json, 0);
			return data.Result;
		}

        public static JSONObject DecodeFromFile(string FileName)
        {
            string json = File.ReadAllText(FileName);
            return Decode(json);
        }

		private struct ScannerData
		{
			public readonly JSONObject Result;
			public readonly int Index;

			public ScannerData(JSONObject result, int index)
			{
				Result = result;
				Index = index;
			}
		}

		private static readonly Dictionary<char, string> EscapeChars =
			new Dictionary<char, string> {
				{ '"', "\"" },
				{ '\\', "\\" },
				{ 'b', "\b" },
				{ 'f', "\f" },
				{ 'n', "\n" },
				{ 'r', "\r" },
				{ 't', "\t" },
			};

		private static ScannerData Scan(string json, int index)
		{
			index = SkipWhitespace(json, index);
			var nextChar = json[index];

			switch (nextChar) {
				case ObjectStart:
					return ScanObject(json, index);
				case ArrayStart:
					return ScanArray(json, index);
				case StringStart:
					return ScanString(json, index + 1);
				case TrueStart:
					return ScanTrue(json, index);
				case FalseStart:
					return ScanFalse(json, index);
				case NullStart:
					return ScanNull(json, index);
				default:
					if (IsNumberStart(nextChar)) {
						return ScanNumber(json, index);
					}
					throw new ParseError("Unexpected token " + nextChar, index);
			}
		}

		private static ScannerData ScanString(string json, int index)
		{
			string s;
			index = ScanBareString(json, index, out s);

			return new ScannerData(JSONObject.CreateString(s), index + 1);
		}

		private static ScannerData ScanTrue(string json, int index)
		{
			return new ScannerData(JSONObject.CreateBoolean(true), ExpectConstant(json, index, "true"));
		}

		private static ScannerData ScanFalse(string json, int index)
		{
			return new ScannerData(JSONObject.CreateBoolean(false), ExpectConstant(json, index, "false"));
		}

		private static ScannerData ScanNull(string json, int index)
		{
			return new ScannerData(JSONObject.CreateNull(), ExpectConstant(json, index, "null"));
		}

		private static ScannerData ScanNumber(string json, int index)
		{
			var negative = false;
			var fractional = false;
			var negativeExponent = false;

			if (json[index] == '-') {
				negative = true;
				++index;
			}

			ulong integerPart = 0;
			if (json[index] != '0') {
				while (json.Length > index && char.IsNumber(json[index])) {
					integerPart = (integerPart * 10) + (ulong)(json[index] - '0');
					++index;
				}
			} else {
				++index;
			}

			ulong fractionalPart = 0;
			int fractionalPartLength = 0;
			if (json.Length > index && json[index] == '.') {
				fractional = true;

				++index;
				while (json.Length > index && char.IsNumber(json[index])) {
					fractionalPart = (fractionalPart * 10) + (ulong)(json[index] - '0');
					++index;
					++fractionalPartLength;
				}
			}

			ulong exponent = 0;
			if (json.Length > index && (json[index] == 'e' || json[index] == 'E')) {
				fractional = true;
				++index;

				if (json[index] == '-') {
					negativeExponent = true;
					++index;
				} else if (json[index] == '+') {
					++index;
				}

				while (json.Length > index && char.IsNumber(json[index])) {
					exponent = (exponent * 10) + (ulong)(json[index] - '0');
					++index;
				}
			}

			return new ScannerData(JSONObject.CreateNumber(negative, fractional, negativeExponent, integerPart, fractionalPart, fractionalPartLength, exponent), index);
		}

		private static ScannerData ScanArray(string json, int index)
		{
			var list = new List<JSONObject>();

			var nextTokenIndex = SkipWhitespace(json, index + 1);
			if (json[nextTokenIndex] == ArrayEnd)
				return new ScannerData(JSONObject.CreateArray(list), nextTokenIndex + 1);

			while (json[index] != ArrayEnd) {
				++index;
				var result = Scan(json, index);
				index = SkipWhitespace(json, result.Index);
				if (json[index] != ArraySeparator && json[index] != ArrayEnd) {
					throw new ParseError("Expecting array separator (,) or array end (])", index);
				}
				list.Add(result.Result);
			}
			return new ScannerData(JSONObject.CreateArray(list), index + 1);
		}

		private static ScannerData ScanObject(string json, int index)
		{			
			var dict = new Dictionary<string, JSONObject>();

			var nextTokenIndex = SkipWhitespace(json, index + 1);
			if (json[nextTokenIndex] == ObjectEnd)
				return new ScannerData(JSONObject.CreateObject(dict), nextTokenIndex + 1);

			while (json[index] != ObjectEnd) {			
				index = SkipWhitespace(json, index + 1);
				if (json[index] != '"') {
					throw new ParseError("Object keys must be strings", index);
				}
				string key;
				index = ScanBareString(json, index + 1, out key) + 1;
				index = SkipWhitespace(json, index);
				if (json[index] != ObjectSeparator) {
					throw new ParseError("Expecting object separator (:)", index);
				}
				++index;
				var valueResult = Scan(json, index);
				index = SkipWhitespace(json, valueResult.Index);
				if (json[index] != ObjectEnd && json[index] != ObjectPairSeparator) {
					throw new ParseError("Expecting object pair separator (,) or object end (})", index);
				}
				dict[key] = valueResult.Result;
			}
			return new ScannerData(JSONObject.CreateObject(dict), index + 1);
		}

		private static int SkipWhitespace(string json, int index)
		{
			while (char.IsWhiteSpace(json[index])) {
				++index;
			}
			return index;
		}

		private static int ExpectConstant(string json, int index, string expected)
		{
			if (json.Substring(index, expected.Length) != expected) {
				throw new ParseError(string.Format("Expected '{0}' got '{1}'",
					expected,
					json.Substring(index, expected.Length)),
					index);
			}
			return index + expected.Length;
		}

		private static bool IsNumberStart(char b)
		{
			return b == '-' || (b >= '0' && b <= '9');
		}

		private static int ScanBareString(string json, int index, out string result)
		{
			// First determine length
			var lengthIndex = index;
			var foundEscape = false;
			while (json[lengthIndex] != '"') {
				if (json[lengthIndex] == '\\') {
					foundEscape = true;
					++lengthIndex;
					if (EscapeChars.ContainsKey(json[lengthIndex]) || json[lengthIndex] == 'u') {
						++lengthIndex;
					} else if (json[lengthIndex] == 'u') {
						lengthIndex += 5;
					}
				} else {
					++lengthIndex;
				}
			}

			if (!foundEscape) {
				result = json.Substring(index, lengthIndex - index);
				return lengthIndex;
			}

			var strBuilder = new StringBuilder(lengthIndex - index);

			var lastIndex = index;
			while (json[index] != '"') {
				if (json[index] == '\\') {
					if (index > lastIndex) {
						strBuilder.Append(json, lastIndex, index - lastIndex);
					}
					++index;
					if (EscapeChars.ContainsKey(json[index])) {
						strBuilder.Append(EscapeChars[json[index]]);
						++index;
					} else if (json[index] == 'u') {
						++index;
						var unicodeSequence = Convert.ToInt32(json.Substring(index, 4), 16);
						strBuilder.Append((char)unicodeSequence);
						index += 4;
					}
					lastIndex = index;
				} else {
					++index;
				}
			}

			if (lastIndex != index) {
				strBuilder.Append(json, lastIndex, index - lastIndex);
			}

			result = strBuilder.ToString();
			return index;
		}
	}
}
/**
 * \endcond
 */
