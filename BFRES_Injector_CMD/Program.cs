﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.Bfres;
using OpenTK;

namespace BFRES_Injector_CMD
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"\n> BFRES Injector v2.0a\n" +
                               "> Made by SMB123W64GB\n" +
                                "> Using Syroot.NintenTools.Bfres API\n" +
                                "> *.bfres *.obj\n");

            ResFile TargetBFRES = new ResFile(args[0]);
            MeshObj test = new MeshObj();
            test.ReadObj(args[1]);
            test.InjectMesh(TargetBFRES.Models[0]);
            TargetBFRES.Models[0].Materials[0].RenderState.PolygonControl.CullBack = false;
            TargetBFRES.Models[0].Materials[0].RenderState.PolygonControl.CullFront = false;
            TargetBFRES.Models[0].Materials[0].RenderState.PolygonControl.PolygonModeEnabled = true;
            TargetBFRES.Name = "A_Cool_Mesh";
            Console.WriteLine("Writing {0}",args[0] + ".new.bfres");
            TargetBFRES.Save(args[0] + ".new.bfres");
        }

        

        public class MeshObj
        {
            public void InjectMesh(Model input)
            {
                //Deal With Mesh
                input.Shapes[0].Meshes[0].Format = Syroot.NintenTools.Bfres.GX2.GX2IndexFormat.UInt16;
                input.Shapes[0].Radius = 100000000f;
                FileOutput MeshByte = new FileOutput();
                input.Shapes[0].Meshes[0].ElementCount = (uint)meshes[0].faces.Count;
                foreach (Vector3 face in meshes[0].faces)
                {
                    MeshByte.writeShort((int)face.X);
                    MeshByte.writeShort((int)face.Y);
                    MeshByte.writeShort((int)face.Z);
                }
                input.Shapes[0].Meshes[0].IndexBuffer.Data[0] = MeshByte.save();
                input.Shapes[0].Meshes[0].ElementCount = (uint)meshes[0].faces.Count*3;
                input.Shapes[0].Meshes[0].SubMeshes.Clear();
                Bounding LeeT = new Bounding();
                LeeT.Center = new Syroot.Maths.Vector3F(0,0,0);
                LeeT.Extent = new Syroot.Maths.Vector3F(100000000f, 100000000f, 100000000f);
                for (int i = 0; i < input.Shapes[0].SubMeshBoundings.Count; i++) input.Shapes[0].SubMeshBoundings[i] = LeeT;

                SubMesh Setup = new SubMesh();
                Setup.Count = (uint)meshes[0].faces.Count * 3;
                Setup.Offset = 0;
                for(int vvv = 0;vvv<4;vvv++) input.Shapes[0].Meshes[0].SubMeshes.Add(Setup);
                input.Shapes[0].Name = meshes[0].Name;
                //Deal with Vertexes
                input.TotalVertexCount = (uint)meshes[0].verts.Count;
                input.Shapes[0].VertexBuffer.VertexCount = (uint)meshes[0].verts.Count;
                //Its no use input.Shapes[0].VertexBuffer.Attributes[1].Format = Syroot.NintenTools.Bfres.GX2.GX2AttribFormat.Format_8_8_8_8_SInt;
                input.Shapes[0].VertexBuffer.Attributes[2].Format = Syroot.NintenTools.Bfres.GX2.GX2AttribFormat.Format_16_16_Single;
                FileOutput VertByte = new FileOutput();
                for (int i = 0;i < meshes[0].verts.Count;i++)
                {
                    VertByte.writeHalfFloat(meshes[0].verts[i].X);
                    VertByte.writeHalfFloat(meshes[0].verts[i].Y);
                    VertByte.writeHalfFloat(meshes[0].verts[i].Z);
                    VertByte.writeHalfFloat(meshes[0].verts[i].W);

                    VertByte.write10sNorm(meshes[0].norms[i]);


                    VertByte.writeHalfFloat(meshes[0].uvs[i].X);
                    VertByte.writeHalfFloat(meshes[0].uvs[i].Y*-1);


                }
                input.Shapes[0].VertexBuffer.Buffers[0].Data[0] = VertByte.save();

                }
            public class SMesh
            {
                public string Name;
                public List<Vector4> verts = new List<Vector4>();
                //Always make the last of the vec4 1.0 (idk what it is)
                public List<Vector4> norms = new List<Vector4>();
                //Last of vec4 is 0 fornow
                public List<Vector2> uvs = new List<Vector2>();
                public List<Vector3> faces = new List<Vector3>();
                public List<List<string>> rawFace = new List<List<string>>();
            }
            public List<SMesh> meshes = new List<SMesh>();
            public void ReadObj(string File)
            {
                List<Vector4> verts = new List<Vector4>();
                List<Vector4> norms = new List<Vector4>();
                List<Vector2> uvs = new List<Vector2>();

                SMesh CMesh = new SMesh();
                string[] text = System.IO.File.ReadAllLines(File);
                //Import the obj
                foreach(string l in text)
                {
                    if(l.Contains("o "))
                    {
                        CMesh = new SMesh();
                        meshes.Add(CMesh);
                        CMesh.Name = l.Remove(0, 2);
                    }
                    if (l.Contains("v "))
                    {
                        string[] vertArry = l.Remove(0, 2).Split(' ');
                        verts.Add(new Vector4(float.Parse(vertArry[0]), float.Parse(vertArry[1]), float.Parse(vertArry[2]),1.0f));
                    }
                    if (l.Contains("vn "))
                    {
                        string[] vertArry = l.Remove(0, 3).Split(' ');
                        norms.Add(new Vector4(float.Parse(vertArry[0]), float.Parse(vertArry[1]), float.Parse(vertArry[2]), 0.0f));
                    }
                    if (l.Contains("vt "))
                    {
                        string[] vertArry = l.Remove(0, 3).Split(' ');
                        uvs.Add(new Vector2(float.Parse(vertArry[0]), float.Parse(vertArry[1])));
                    }
                    if (l.Contains("f "))
                    {
                        string[] vertArry = l.Remove(0, 2).Split(' ');
                        if (vertArry.Length > 3) Console.WriteLine("Non-Traingulated Mesh\tPlease Re-Export with triangulated Mesh");
                        List<string> faceArray = new List<string>();
                        CMesh.rawFace.Add(new List<string>(vertArry.OfType<string>().ToList()));
                    }
                }
                //Time To Clean Our OBJ
                foreach(SMesh sm in meshes)
                {
                    List<Vector3> points = new List<Vector3>();
                    Dictionary<string, int> lookup = new Dictionary<string, int>();
                    int fi = 0;
                    foreach(List<string> faceRaw in sm.rawFace)
                    {
                        List<int> trueface = new List<int>();
                        foreach(string f in faceRaw)
                        {
                            if (!lookup.ContainsKey(f))
                            {
                                lookup.Add(f, fi);
                                trueface.Add(fi++);
                                string[] sr = f.Split('/');
                                sm.verts.Add(verts[int.Parse(sr[0]) - 1]);
                                sm.uvs.Add(uvs[int.Parse(sr[1]) - 1]);
                                sm.norms.Add(norms[int.Parse(sr[2]) - 1]);
                            }
                            else
                                trueface.Add(lookup[f]);
                            

                        }
                        sm.faces.Add(new Vector3((float)trueface[0], (float)trueface[1], (float)trueface[2]));
                    }
                }

            }

        }
        public class FileOutput
        {
            public static int fromFloat(float fval, bool littleEndian)
            {
                int fbits = FileOutput.SingleToInt32Bits(fval, littleEndian);
                int sign = fbits >> 16 & 0x8000;          // sign only
                int val = (fbits & 0x7fffffff) + 0x1000; // rounded value

                if (val >= 0x47800000)               // might be or become NaN/Inf
                {                                     // avoid Inf due to rounding
                    if ((fbits & 0x7fffffff) >= 0x47800000)
                    {                                 // is or must become NaN/Inf
                        if (val < 0x7f800000)        // was value but too large
                            return sign | 0x7c00;     // make it +/-Inf
                        return sign | 0x7c00 |        // remains +/-Inf or NaN
                            (fbits & 0x007fffff) >> 13; // keep NaN (and Inf) bits
                    }
                    return sign | 0x7bff;             // unrounded not quite Inf
                }
                if (val >= 0x38800000)               // remains normalized value
                    return sign | val - 0x38000000 >> 13; // exp - 127 + 15
                if (val < 0x33000000)                // too small for subnormal
                    return sign;                      // becomes +/-0
                val = (fbits & 0x7fffffff) >> 23;  // tmp exp for subnormal calc
                return sign | ((fbits & 0x7fffff | 0x800000) // add subnormal bit
                    + (0x800000 >> val - 102)     // round depending on cut off
                    >> 126 - val);   // div by 2^(1-(exp-127+15)) and >> 13 | exp=0
            }
            List<byte> data = new List<byte>();

            public byte[] getBytes()
            {
                return data.ToArray();
            }

            public void writeString(String s)
            {
                char[] c = s.ToCharArray();
                for (int i = 0; i < c.Length; i++)
                    data.Add((byte)c[i]);
            }

            public int size()
            {
                return data.Count;
            }

            public void writeOutput(FileOutput d)
            {
                foreach (byte b in d.data)
                    data.Add(b);
            }

            private static char[] HexToCharArray(string hex)
            {
                return Enumerable.Range(0, hex.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                 .Select(x => Convert.ToChar(x))
                                 .ToArray();
            }

            public void writeHex(string s)
            {
                char[] c = HexToCharArray(s);
                for (int i = 0; i < c.Length; i++)
                    data.Add((byte)c[i]);
            }

            public void writeInt(int i)
            {

                data.Add((byte)((i >> 24) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));

            }

            public void writeIntAt(int i, int p)
            {
                data[p++] = (byte)((i >> 24) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
            public void writeShortAt(int i, int p)
            {
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }

            public void align(int i)
            {
                while (data.Count % i != 0)
                    writeByte(0);
            }

            public void align(int i, int v)
            {
                while (data.Count % i != 0)
                    writeByte(v);
            }

            /*public void align(int i, int value){
                while(data.size() % i != 0)
                    writeByte(value);
            }*/


            public void writeFloat(float f)
            {
                int i = SingleToInt32Bits(f, false);
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 24) & 0xFF));
            }

            public void writeFloatAt(float f, int p)
            {
                int i = SingleToInt32Bits(f, true);
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 24) & 0xFF);
            }

            public static int SingleToInt32Bits(float value, bool littleEndian)
            {
                byte[] b = BitConverter.GetBytes(value);
                int p = 0;

                if (!littleEndian)
                {
                    return (b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 24);
                }
                else
                    return ((b[p++] & 0xFF) << 24) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 8) | (b[p++] & 0xFF);
            }

            public void writeHalfFloat(float f)
            {
                int i = fromFloat(f, false);
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }

            public void writeShort(int i)
            {
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));

            }

            public void writeByte(int i)
            {
                data.Add((byte)((i) & 0xFF));
            }
            public void writeSByte(int i)
            {

                 
                data.Add((byte)((i) >> 0x1F + ((i) & 0x7F)));
            }

            public void writeChars(char[] c)
            {
                foreach (char ch in c)
                    writeByte(Convert.ToByte(ch));
            }

            public void writeBytes(byte[] bytes)
            {
                foreach (byte b in bytes)
                    writeByte(b);
            }

            public void write10sNorm(Vector4 vec)
            {
                //Vector3 norm = new Vector3(~(((int)(vec.X*511)*-1)-1), ~(((int)(vec.Y*551) * -1) - 1),~(((int)(vec.Z*511) * -1) - 1));
                Vector3 norm = new Vector3((int)(vec.X * 511), (int)(vec.Y * 511), (int)(vec.Z * 511));
                int Normal = ((int)norm.X) + (((int)norm.Y) << 10) + (((int)norm.Z) << 20);
                writeInt(Normal);
            }

            public void writeFlag(bool b)
            {
                if (b)
                    writeByte(1);
                else
                    writeByte(0);
            }

            public int pos()
            {
                return data.Count;
            }

            public byte[] save()
            {
                return data.ToArray();
            }
        }
    }
}
