using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using System.Linq;

public enum DecodeType
{
    EightBit,
    BigEndian,
    Dcm,
}

public class Make3dTex : MonoBehaviour
{
    [MenuItem("CreateVoxelGrid/3DTexture")]
    public static void CreateAllTextures(){
        //CreateTexture3D("/bunny512x512x361.raw","BunnyTex",512,512,361, DecodeType.EightBit, 1);
        CreateTexture3D("/bucky32x32x32.raw","BuckyTex",32,32,32, DecodeType.EightBit, 1);
        CreateTexture3D("/engine256x256x256.raw","EngineTex",256,256,256, DecodeType.EightBit, 1);
        CreateTexture3D("/hnut256_uint.raw","HazelTex",256,256,256, DecodeType.EightBit, 1);
        CreateTexture3D("/cthead256x256x113.raw","HeadTex",256,256,113, DecodeType.BigEndian, 20);
        CreateTexture3D("/torso512x512x189.raw","TorsoTex",512,512,189, DecodeType.Dcm, 25);
    }

    
    public static void CreateTexture3D(String filepath, String filename, int xsize, int ysize, int zsize, DecodeType decodeType, int scaling)
    {
        Debug.Log("Creating 3DTex for " + filepath);
        //configure file path
        float[,,] grid = getGridFromFile(filepath,xsize,ysize, zsize, decodeType, scaling);
        
        // Create the texture and apply the configuration
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;
        Texture3D texture = new Texture3D(xsize, ysize, zsize, format, false);
        texture.wrapMode = wrapMode;

        for (int z = 0; z < zsize; z++)
        {
            for (int y = 0; y < ysize; y++)
            {
                for (int x = 0; x < xsize; x++)
                {
                    texture.SetPixel(x,y,z,new Color(0.0f,
                        0.0f, 0.0f, (float)grid[x,y,z]));
                }
            }
        }
        
        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();        

        // Save the texture
        AssetDatabase.CreateAsset(texture, "Assets/_Project/Resources/3DTex/" + filename + ".asset");
    }

    private static float[,,] getGridFromFile(string path, int sizeX, int sizeY, int sizeZ, DecodeType decodeType, int scaling)
    {
        // And read the binary file
        float minValue = 1;
        float maxValue = 0;
        BinaryReader binReader = new BinaryReader(File.Open((Application.streamingAssetsPath + path), FileMode.Open));
        float[,,] grid = new float[sizeX, sizeY, sizeZ];
        for (int z = 0; z < sizeZ; z++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    switch(decodeType)
                    {
                        case DecodeType.EightBit:
                            grid[x, y, z] = binReader.ReadByte();
                            grid[x, y, z] /= 255;
                            break;
                        case DecodeType.BigEndian:
                            byte msb = binReader.ReadByte();  // most significant byte (big-endian)
                            byte lsb = binReader.ReadByte();  // least significant byte
                            ushort value = (ushort)((msb << 8) | lsb);
                            grid[x, y, z] = value / 65535f * scaling;
                            break;
                        case DecodeType.Dcm:
                            ushort raw = binReader.ReadUInt16();
                            grid[x, y, z] = raw / 65535f * scaling;
                            break;
                    }
                    if(grid[x, y, z] > 1.0f){
                        grid[x, y, z] = 1.0f;
                    }
                    if (grid[x, y, z] > maxValue){
                        maxValue = grid[x, y, z];
                    }
                    if (grid[x, y, z] < minValue){
                        minValue = grid[x, y, z];
                    }
                    
                }
            }
        }
        Debug.Log("Minvalue: " + minValue + " MaxValue: " + maxValue);
        binReader.Close();

        return grid;
    }


    [MenuItem("CreateVoxelGrid/NormalMap")]
    public static void CreateNormalMap()
    {
        //set dimensions
        int xsize = 512;
        int ysize = 512;
        int zsize = 361;
        
        //get voxel grid from assets
        float[,,] grid = getGridFromFile("/bunny512x512x361.raw", xsize,ysize,zsize, DecodeType.EightBit, 1);
        //create new 3d texture
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;
        Texture3D texture = new Texture3D(xsize, ysize, zsize, format, false);
        texture.wrapMode = wrapMode;
        //loop through grid and set normal as color in 3d texture
        int delta = 3;
        for (int z = delta; z < zsize-delta; z++)
        {
            for (int y = delta; y < ysize-delta; y++)
            {
                for (int x = delta; x < xsize-delta; x++)
                {
                    texture.SetPixel(x,y,z,centralDiffColor(grid,x,y,z,xsize,ysize,zsize, delta));
                }
            }
        }
        
        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();        

        // Save the texture
        AssetDatabase.CreateAsset(texture, "Assets/_Project/Resources/3DTex/bunny_normals.asset");
    }

    private static Color centralDiffColor(float[,,] grid, int x, int y, int z, int xsize, int ysize, int zsize, int delta)
    {
        Color color = new Color(0,0,0,1);
        try
            {
                //TODO: apply transfer function and use alpha channel
                //use central differences to make gradient vectors
                float xdif = grid[x + delta, y, z] - grid[x - delta, y, z];
                float ydif = grid[x, y + delta, z] - grid[x, y - delta, z];
                float zdif = grid[x, y, z + delta] - grid[x, y, z - delta];
                color = new Color(xdif, ydif, zdif, 0);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                UnityEngine.Debug.Log("vars: " + x + " " + y + " " + z);
            }
        return color;
    }

    [MenuItem("CreateVoxelGrid/ExportGridCSV")]
    public static void ExportAllCSVs()
    {
        CreateCSV("/bunny512x512x361.raw","BunnyCSV",512,512,361, DecodeType.EightBit, 1);
        CreateCSV("/bucky32x32x32.raw","BuckyCSV",32,32,32, DecodeType.EightBit, 1);
        CreateCSV("/engine256x256x256.raw","EngineCSV",256,256,256, DecodeType.EightBit, 1);
        CreateCSV("/hnut256_uint.raw","HazelCSV",256,256,256, DecodeType.EightBit, 1);
        CreateCSV("/cthead256x256x113.raw","HeadCSV",256,256,113, DecodeType.BigEndian, 20);
        CreateCSV("/torso512x512x189.raw","TorsoCSV",512,512,189, DecodeType.Dcm, 25);
    }

    public static void CreateCSV(String filepath, String filename, int xsize, int ysize, int zsize, DecodeType decodeType, int scaling)
    {
        Debug.Log("Creating CSV for " + filepath);
        float[,,] grid = getGridFromFile(filepath,xsize,ysize, zsize, decodeType, scaling);

        // 1) decide your output path
        string outPath = Path.Combine(
            Application.dataPath,     // your project’s Assets folder
            "../" + filename + ".csv"       // one level up, so it lands at the project root
        );

        // 2) stream it out
        using (var sw = new StreamWriter(outPath))
        {
            // header
            sw.WriteLine("x,y,z,value");

            // one row per voxel!
            for (int z = 0; z < zsize; z++)
            for (int y = 0; y < ysize; y++)
            for (int x = 0; x < xsize; x++)
            {
                // if you still want to ignore low values:
                // if (grid[x,y,z] < 0.07f) continue;

                sw.WriteLine($"{x},{y},{z},{grid[x,y,z]:G6}");
            }
        }

        Debug.Log($"Exported volume to CSV: {outPath}");
        AssetDatabase.Refresh();
    }
}
