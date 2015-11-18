using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace SecureFileShare.Security.APIs
{
    //Got it from http://www.codeproject.com/Articles/531028/Encrypted-code-compiled-at-runtime
    public class MyManagementClass
    {
        private readonly byte[] ecode =
        {
            00,
        };

        public object NewClass(byte[] key, string classname)
        {
            Assembly asm = CompileEncryptedCode(ecode, key);
            return asm.CreateInstance(classname);
        }

        public object CallMethod(byte[] key, string classname, string methodname, params object[] methodparameters)
        {
            Assembly asm = CompileEncryptedCode(ecode, key);
            object obj = asm.CreateInstance(classname);
            object result = obj.GetType()
                .InvokeMember(methodname, BindingFlags.InvokeMethod, null, obj, methodparameters);
            return result;
        }

        #region encryption methods

        private static Assembly CompileEncryptedCode(byte[] ecode, byte[] key)
        {
            byte[] dcode = RijndaelEncryption(ecode, key, false);
            string code = Encoding.ASCII.GetString(dcode);
            var provider = new CSharpCodeProvider();
            var compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = false;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string location = assembly.Location;
                    if (!String.IsNullOrEmpty(location))
                    {
                        compilerparams.ReferencedAssemblies.Add(location);
                    }
                }
                catch (NotSupportedException)
                {
                    // this happens for dynamic assemblies: ignore it.                         
                }
            }
            CompilerResults results = provider.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                var errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n",
                        error.Line, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            AppDomain.CurrentDomain.Load(results.CompiledAssembly.GetName());
            return results.CompiledAssembly;
        }

        private static byte[] RijndaelEncryption(byte[] input, byte[] key, bool encrypt, bool parallel = true)
        {
            var algorithm = new Rijndael();
            int InputBlockLen_byte = algorithm.InputBlockLen_bit/8;
            byte[] expandedkey = algorithm.ExpandKey(key);

            if (input.Length > 0 && key.Length > 0)
            {
                int countblocks;
                byte[] output;
                if (encrypt)
                {
                    int bytestoadd = (InputBlockLen_byte - (input.Length + 1)%InputBlockLen_byte)%InputBlockLen_byte;
                    int newinputlen = input.Length + bytestoadd + 1;
                    Array.Resize(ref input, newinputlen);
                    input[input.Length - 1] = (byte) bytestoadd;
                    countblocks = newinputlen/InputBlockLen_byte;
                    output = new byte[newinputlen];
                }
                else
                {
                    countblocks = input.Length/InputBlockLen_byte;
                    output = new byte[input.Length];
                }
                if (parallel)
                {
                    Parallel.For(0, countblocks, delegate(int b)
                    {
                        var block = new byte[InputBlockLen_byte];
                        Buffer.BlockCopy(input, b*InputBlockLen_byte, block, 0, InputBlockLen_byte);
                        block = encrypt
                            ? algorithm.EncryptBlock(block, expandedkey)
                            : algorithm.DecryptBlock(block, expandedkey);
                        Buffer.BlockCopy(block, 0, output, b*InputBlockLen_byte, InputBlockLen_byte);
                    });
                }
                else
                {
                    var block = new byte[InputBlockLen_byte];
                    for (int b = 0; b < countblocks; b++)
                    {
                        Buffer.BlockCopy(input, b*InputBlockLen_byte, block, 0, InputBlockLen_byte);
                        block = encrypt
                            ? algorithm.EncryptBlock(block, expandedkey)
                            : algorithm.DecryptBlock(block, expandedkey);
                        Buffer.BlockCopy(block, 0, output, b*InputBlockLen_byte, InputBlockLen_byte);
                    }
                }
                if (!encrypt)
                {
                    int countbytestoerase = output[output.Length - 1] + 1;

                    int newoutputlenght = output.Length - countbytestoerase;
                    Array.Resize(ref output, newoutputlenght);
                }

                return output;
            }
            return null;
        }

        private class Rijndael
        {
            private static byte[] Rcon =
            {
                0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
                0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39,
                0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a,
                0x74, 0xe8, 0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8,
                0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef,
                0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc,
                0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b,
                0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3,
                0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94,
                0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20,
                0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35,
                0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f,
                0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d, 0x01, 0x02, 0x04,
                0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63,
                0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd,
                0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d
            };

            public static readonly byte[] Sbox =
            {
                0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B, 0xFE, 0xD7, 0xAB, 0x76,
                0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF, 0x9C, 0xA4, 0x72, 0xC0,
                0xB7, 0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1, 0x71, 0xD8, 0x31, 0x15,
                0x04, 0xC7, 0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2, 0xEB, 0x27, 0xB2, 0x75,
                0x09, 0x83, 0x2C, 0x1A, 0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3, 0x29, 0xE3, 0x2F, 0x84,
                0x53, 0xD1, 0x00, 0xED, 0x20, 0xFC, 0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39, 0x4A, 0x4C, 0x58, 0xCF,
                0xD0, 0xEF, 0xAA, 0xFB, 0x43, 0x4D, 0x33, 0x85, 0x45, 0xF9, 0x02, 0x7F, 0x50, 0x3C, 0x9F, 0xA8,
                0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D, 0x38, 0xF5, 0xBC, 0xB6, 0xDA, 0x21, 0x10, 0xFF, 0xF3, 0xD2,
                0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44, 0x17, 0xC4, 0xA7, 0x7E, 0x3D, 0x64, 0x5D, 0x19, 0x73,
                0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88, 0x46, 0xEE, 0xB8, 0x14, 0xDE, 0x5E, 0x0B, 0xDB,
                0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2, 0xD3, 0xAC, 0x62, 0x91, 0x95, 0xE4, 0x79,
                0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56, 0xF4, 0xEA, 0x65, 0x7A, 0xAE, 0x08,
                0xBA, 0x78, 0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74, 0x1F, 0x4B, 0xBD, 0x8B, 0x8A,
                0x70, 0x3E, 0xB5, 0x66, 0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9, 0x86, 0xC1, 0x1D, 0x9E,
                0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9, 0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9, 0xCE, 0x55, 0x28, 0xDF,
                0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68, 0x41, 0x99, 0x2D, 0x0F, 0xB0, 0x54, 0xBB, 0x16
            };

            public static readonly byte[] invSbox =
            {
                0x52, 0x09, 0x6A, 0xD5, 0x30, 0x36, 0xA5, 0x38, 0xBF, 0x40, 0xA3, 0x9E, 0x81, 0xF3, 0xD7, 0xFB,
                0x7C, 0xE3, 0x39, 0x82, 0x9B, 0x2F, 0xFF, 0x87, 0x34, 0x8E, 0x43, 0x44, 0xC4, 0xDE, 0xE9, 0xCB,
                0x54, 0x7B, 0x94, 0x32, 0xA6, 0xC2, 0x23, 0x3D, 0xEE, 0x4C, 0x95, 0x0B, 0x42, 0xFA, 0xC3, 0x4E,
                0x08, 0x2E, 0xA1, 0x66, 0x28, 0xD9, 0x24, 0xB2, 0x76, 0x5B, 0xA2, 0x49, 0x6D, 0x8B, 0xD1, 0x25,
                0x72, 0xF8, 0xF6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xD4, 0xA4, 0x5C, 0xCC, 0x5D, 0x65, 0xB6, 0x92,
                0x6C, 0x70, 0x48, 0x50, 0xFD, 0xED, 0xB9, 0xDA, 0x5E, 0x15, 0x46, 0x57, 0xA7, 0x8D, 0x9D, 0x84,
                0x90, 0xD8, 0xAB, 0x00, 0x8C, 0xBC, 0xD3, 0x0A, 0xF7, 0xE4, 0x58, 0x05, 0xB8, 0xB3, 0x45, 0x06,
                0xD0, 0x2C, 0x1E, 0x8F, 0xCA, 0x3F, 0x0F, 0x02, 0xC1, 0xAF, 0xBD, 0x03, 0x01, 0x13, 0x8A, 0x6B,
                0x3A, 0x91, 0x11, 0x41, 0x4F, 0x67, 0xDC, 0xEA, 0x97, 0xF2, 0xCF, 0xCE, 0xF0, 0xB4, 0xE6, 0x73,
                0x96, 0xAC, 0x74, 0x22, 0xE7, 0xAD, 0x35, 0x85, 0xE2, 0xF9, 0x37, 0xE8, 0x1C, 0x75, 0xDF, 0x6E,
                0x47, 0xF1, 0x1A, 0x71, 0x1D, 0x29, 0xC5, 0x89, 0x6F, 0xB7, 0x62, 0x0E, 0xAA, 0x18, 0xBE, 0x1B,
                0xFC, 0x56, 0x3E, 0x4B, 0xC6, 0xD2, 0x79, 0x20, 0x9A, 0xDB, 0xC0, 0xFE, 0x78, 0xCD, 0x5A, 0xF4,
                0x1F, 0xDD, 0xA8, 0x33, 0x88, 0x07, 0xC7, 0x31, 0xB1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xEC, 0x5F,
                0x60, 0x51, 0x7F, 0xA9, 0x19, 0xB5, 0x4A, 0x0D, 0x2D, 0xE5, 0x7A, 0x9F, 0x93, 0xC9, 0x9C, 0xEF,
                0xA0, 0xE0, 0x3B, 0x4D, 0xAE, 0x2A, 0xF5, 0xB0, 0xC8, 0xEB, 0xBB, 0x3C, 0x83, 0x53, 0x99, 0x61,
                0x17, 0x2B, 0x04, 0x7E, 0xBA, 0x77, 0xD6, 0x26, 0xE1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0C, 0x7D
            };

            private readonly int InputBlockLen_Byte;
            private readonly int KeyLen_Byte;
            private readonly int KeyLen_bit;
            private readonly int _inputBlockLen_bit;
            private readonly int numberOfRounds;
            private int expandedKeyLen_Byte;

            public Rijndael()
            {
                _inputBlockLen_bit = 256;
                KeyLen_bit = 256;
                InputBlockLen_Byte = _inputBlockLen_bit/8;
                KeyLen_Byte = KeyLen_bit/8;
                numberOfRounds = Math.Max((KeyLen_Byte/4 + 6), (InputBlockLen_Byte/4 + 6));
                expandedKeyLen_Byte = (numberOfRounds + 1)*16;
            }

            public int InputBlockLen_bit
            {
                get { return _inputBlockLen_bit; }
            }

            private static byte rcon(byte input)
            {
                byte result = 1;
                if (input == 0)
                    return 0;
                while (input != 1)
                {
                    byte b;
                    b = ((byte) (result & 0x80));
                    result <<= 1;
                    if (b == 0x80)
                    {
                        result ^= 0x1b;
                    }
                    input--;
                }
                return result;
            }

            private byte[] SubBytes(byte[] input)
            {
                var result = new byte[InputBlockLen_Byte];
                int j;
                for (j = 0; j < InputBlockLen_Byte; j++)
                {
                    result[j] = Sbox[input[j]];
                }
                return result;
            }

            private byte[] InvSubBytes(byte[] input)
            {
                var result = new byte[InputBlockLen_Byte];
                int j;
                for (j = 0; j < InputBlockLen_Byte; j++)
                {
                    result[j] = invSbox[input[j]];
                }
                return result;
            }

            private byte[] ShiftRows(byte[] input)
            {
                var result = new byte[InputBlockLen_Byte];
                int j, k;
                int[] shift;
                if (InputBlockLen_bit > 192)
                {
                    shift = new[] {0, 1, 3, 4};
                }
                else
                {
                    shift = new[] {0, 1, 2, 3};
                }
                var tempblock = new byte[InputBlockLen_Byte];
                Buffer.BlockCopy(input, 0, tempblock, 0, InputBlockLen_Byte);
                for (j = 0; j < InputBlockLen_Byte; j += 4)
                {
                    for (k = 0; k <= 3; k++)
                    {
                        result[j + k] = tempblock[(j + k + shift[k]*4)%InputBlockLen_Byte];
                    }
                }
                return result;
            }

            private byte[] InvShiftRows(byte[] input)
            {
                var result = new byte[InputBlockLen_Byte];
                int j, k;

                int[] shift;
                if (InputBlockLen_bit > 192)
                {
                    shift = new[] {0, 7, 5, 4};
                }
                else
                {
                    shift = new[] {0, 7, 6, 5};
                }
                var tempblock = new byte[InputBlockLen_Byte];
                Buffer.BlockCopy(input, 0, tempblock, 0, InputBlockLen_Byte);
                for (j = 0; j < InputBlockLen_Byte; j += 4)
                {
                    for (k = 0; k <= 3; k++)
                    {
                        result[j + k] = tempblock[(j + k + shift[k]*4)%InputBlockLen_Byte];
                    }
                }
                return result;
            }

            private byte[] MixColumns(byte[] input)
            {
                var result = new byte[InputBlockLen_Byte];

                int j, t;
                var bmul = new byte[4, 4];
                for (j = 0; j < InputBlockLen_Byte; j += 4)
                {
                    for (t = 0; t < 4; t++)
                    {
                        bmul[1, t] = input[j + t];
                        bmul[2, t] = (byte) (bmul[1, t] << 1);
                        if ((bmul[1, t] & 0x80) > 0)
                        {
                            bmul[2, t] = (byte) (bmul[2, t] ^ 0x1b);
                        }
                        bmul[3, t] = (byte) (bmul[2, t] ^ bmul[1, t]);
                    }
                    for (t = 0; t < 4; t++)
                    {
                        result[j + t] =
                            (byte) (bmul[2, t] ^ bmul[3, (t + 1)%4] ^ bmul[1, (t + 2)%4] ^ bmul[1, (t + 3)%4]);
                    }
                }
                return result;
            }

            private byte[] InvMixColumns(byte[] input)
            {
                var result = new byte[InputBlockLen_Byte];

                int j, t;
                var bmul = new byte[15, 4];
                for (j = 0; j < InputBlockLen_Byte; j += 4)
                {
                    for (t = 0; t < 4; t++)
                    {
                        bmul[1, t] = input[j + t];
                        bmul[2, t] = (byte) (bmul[1, t] << 1);
                        if ((bmul[1, t] & 0x80) > 0)
                        {
                            bmul[2, t] = (byte) (bmul[2, t] ^ 0x1b);
                        }
                        bmul[3, t] = (byte) (bmul[2, t] ^ bmul[1, t]);

                        bmul[4, t] = (byte) (bmul[2, t] << 1);
                        if ((byte) (bmul[2, t] & 0x80) > 0)
                        {
                            bmul[4, t] = (byte) (bmul[4, t] ^ 0x1b);
                        }
                        bmul[8, t] = (byte) (bmul[4, t] << 1);
                        if ((byte) (bmul[4, t] & 0x80) > 0)
                        {
                            bmul[8, t] = (byte) (bmul[8, t] ^ 0x1b);
                        }
                        bmul[9, t] = (byte) (bmul[8, t] ^ bmul[1, t]);
                        bmul[11, t] = (byte) (bmul[8, t] ^ bmul[2, t] ^ bmul[1, t]);
                        bmul[13, t] = (byte) (bmul[8, t] ^ bmul[4, t] ^ bmul[1, t]);
                        bmul[14, t] = (byte) (bmul[8, t] ^ bmul[4, t] ^ bmul[2, t]);
                    }
                    for (t = 0; t < 4; t++)
                    {
                        result[j + t] =
                            (byte) (bmul[14, t] ^ bmul[11, (t + 1)%4] ^ bmul[13, (t + 2)%4] ^ bmul[9, (t + 3)%4]);
                    }
                }
                return result;
            }

            private byte[] XorRoundKey(byte[] input, byte[] key)
            {
                var result = new byte[InputBlockLen_Byte];
                for (int j = 0; j < InputBlockLen_Byte; j++)
                {
                    result[j] = (byte) (input[j] ^ key[j]);
                }
                return result;
            }

            public byte[] ExpandKey(byte[] key)
            {
                var nkey = new byte[KeyLen_Byte];

                Buffer.BlockCopy(key, 0, nkey, 0, Math.Min(key.Length, KeyLen_Byte));
                if (key.Length < KeyLen_Byte)
                {
                    int k = 0;
                    for (int pi = key.Length; pi < KeyLen_Byte; pi++)
                    {
                        nkey[pi] = nkey[k++];
                    }
                }
                bool addextrasbox = InputBlockLen_bit >= 256;
                if (KeyLen_bit%64 != 0 || KeyLen_bit < 128)
                {
                    return null;
                }
                int expandedKeyByteLenght = (numberOfRounds + 1)*InputBlockLen_Byte;
                var expkey = new byte[expandedKeyByteLenght];
                Buffer.BlockCopy(nkey, 0, expkey, 0, Math.Min(nkey.Length, KeyLen_Byte));
                nkey.CopyTo(expkey, 0);
                var tmp = new byte[4];
                int c = KeyLen_Byte;
                byte i = 1;
                byte a;
                while (c < expandedKeyByteLenght)
                {
                    for (a = 0; a < 4; a++)
                    {
                        tmp[a] = expkey[a + c - 4];
                    }
                    if (c%KeyLen_Byte == 0)
                    {
                        #region ScrambleKey

                        byte first = tmp[0];
                        Buffer.BlockCopy(tmp, 1, tmp, 0, (tmp.Length - 1));
                        tmp[tmp.Length - 1] = first;

                        for (byte b = 0; b < 4; b++)
                        {
                            tmp[b] = Sbox[tmp[b]];
                        }
                        tmp[0] ^= rcon(i);

                        #endregion

                        i++;
                    }
                    if (c%32 == 16 && addextrasbox)
                    {
                        for (a = 0; a < 4; a++)
                        {
                            tmp[a] = Sbox[tmp[a]];
                        }
                    }
                    for (a = 0; a < 4; a++)
                    {
                        expkey[c] = (byte) (expkey[c - KeyLen_Byte] ^ tmp[a]);
                        c++;
                    }
                }

                return expkey;
            }

            public byte[] EncryptBlock(byte[] input, byte[] key)
            {
                var result = new byte[input.Length];
                Buffer.BlockCopy(input, 0, result, 0, input.Length);
                var currentkey = new byte[input.Length];
                int round = 0;
                Buffer.BlockCopy(key, round, currentkey, 0, input.Length);
                result = XorRoundKey(result, currentkey);
                for (round = 1; round < numberOfRounds; round++)
                {
                    Buffer.BlockCopy(key, round*InputBlockLen_Byte, currentkey, 0, input.Length);
                    result = SubBytes(result);
                    result = ShiftRows(result);
                    result = MixColumns(result);
                    result = XorRoundKey(result, currentkey);
                }
                Buffer.BlockCopy(key, round*InputBlockLen_Byte, currentkey, 0, input.Length);
                result = SubBytes(result);
                result = ShiftRows(result);
                result = XorRoundKey(result, currentkey);
                return result;
            }

            public byte[] DecryptBlock(byte[] input, byte[] key)
            {
                var result = new byte[input.Length];
                Buffer.BlockCopy(input, 0, result, 0, input.Length);
                var currentkey = new byte[input.Length];
                int round = numberOfRounds;
                Buffer.BlockCopy(key, round*InputBlockLen_Byte, currentkey, 0, input.Length);
                result = XorRoundKey(result, currentkey);
                result = InvShiftRows(result);
                result = InvSubBytes(result);
                round--;
                while (round > 0)
                {
                    Buffer.BlockCopy(key, round*InputBlockLen_Byte, currentkey, 0, input.Length);
                    result = XorRoundKey(result, currentkey);
                    result = InvMixColumns(result);
                    result = InvShiftRows(result);
                    result = InvSubBytes(result);
                    round--;
                }
                Buffer.BlockCopy(key, 0, currentkey, 0, input.Length);
                result = XorRoundKey(result, currentkey);
                return result;
            }
        }

        #endregion
    }
}