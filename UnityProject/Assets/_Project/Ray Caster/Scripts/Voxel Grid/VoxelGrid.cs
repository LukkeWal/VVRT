using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using _Project.Ray_Caster.Scripts.RC_Ray;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace _Project.Ray_Caster.Scripts.Voxel_Grid
{
    /// <summary>
    /// Class tied to the voxel grid prefab. It hold the voxel grid data and performs actions related to the voxel grid
    /// </summary>
    public class VoxelGrid : MonoBehaviour
    {
        private static VoxelGrid instance = null;
        private RayCasterManager rayCasterManager = null;

        /// <summary>
        /// The grid containing the selected voxelgrids scalar data
        /// </summary>
        public float[,,] Grid;
        /// <summary>
        /// Size of the voxel grid
        /// </summary>
        public int SizeX = 100;
        public int SizeY = 100;
        public int SizeZ = 100;

        /// <summary>
        /// The recommended transfer function for the selected voxel grid
        /// </summary>
        public RCRay.ColorTableEntry[] RecommendedColorLookupTable;

        /// <summary>
        /// UI elements used to display the progress of loading a new voxel grid
        /// </summary>
        [SerializeField] private GameObject loadVoxelGridProgressFill;
        [SerializeField] private TextMeshProUGUI loadVoxelGridtaskProgress;
        [SerializeField] private VoxelGridType selectedVoxelGrid;
        [SerializeField] private GameObject progressBar;

        /// <summary>
        /// The selected voxel grid type
        /// </summary>
        public VoxelGridType SelectedVoxelGrid
        {
            get { return selectedVoxelGrid; }
            set
            {
                if (value == selectedVoxelGrid) return;
                selectedVoxelGrid = value;
            }
        }

        // we remember previously generated voxel grids to speed up the game when the user is switching between grids
        private float[,,] buckyGrid = null;
        private float[,,] bunnyGrid = null;
        private float[,,] engineGrid = null;
        private float[,,] hazelnutGrid = null;
        private float[,,] headGrid = null;
        private float[,,] torsoGrid = null;


        /// <summary>
        /// The available voxel grid types
        /// </summary>
        public enum VoxelGridType
        {
            Bucky,
            Engine,
            Hazelnut,
            Head,
            Torso,
            Bunny
        }

        /// <summary>
        /// Get the recommended color lookup table for the selected voxel grid
        /// </summary>
        /// <param name="type">The voxel grid type to get the recommended color lookup table for</param>
        /// <returns>The recommended color lookup table</returns>
        private RCRay.ColorTableEntry[] getReccomendedColorLookupTable(VoxelGridType type)
        {
            RecommendedColorLookupTable = new RCRay.ColorTableEntry[5];
            switch (type)
            {
                case VoxelGridType.Bucky:
                    RecommendedColorLookupTable[0].Density = 0.2f;
                    RecommendedColorLookupTable[1].Density = 0.4f;
                    RecommendedColorLookupTable[2].Density = 0.7f;
                    RecommendedColorLookupTable[3].Density = 0.9f;
                    RecommendedColorLookupTable[4].Density = 1.0f;
                    RecommendedColorLookupTable[0].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[1].ColorAlpha = new Color(0, 0, 0, 0.3f);
                    RecommendedColorLookupTable[2].ColorAlpha = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    RecommendedColorLookupTable[3].ColorAlpha = new Color(1, 1, 1, 0.3f);
                    RecommendedColorLookupTable[4].ColorAlpha = new Color(1, 1, 1, 0.3f);
                    return RecommendedColorLookupTable;
                case VoxelGridType.Bunny:
                    RecommendedColorLookupTable[0].Density = 0f;
                    RecommendedColorLookupTable[1].Density = 0f;
                    RecommendedColorLookupTable[2].Density = 0f;
                    RecommendedColorLookupTable[3].Density = 0.568f;
                    RecommendedColorLookupTable[4].Density = 1.0f;
                    RecommendedColorLookupTable[0].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[1].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[2].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[3].ColorAlpha = new Color(0, 1, 0, 0.3f);
                    RecommendedColorLookupTable[4].ColorAlpha = Color.clear;
                    return RecommendedColorLookupTable;
                case VoxelGridType.Hazelnut:
                    RecommendedColorLookupTable[0].Density = 0.06f;
                    RecommendedColorLookupTable[1].Density = 0.07f;
                    RecommendedColorLookupTable[2].Density = 0.32f;
                    RecommendedColorLookupTable[3].Density = 0.33f;
                    RecommendedColorLookupTable[4].Density = 0.42f;
                    RecommendedColorLookupTable[0].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[1].ColorAlpha = new Color(0, 1, 0, 0.3f);
                    RecommendedColorLookupTable[2].ColorAlpha = new Color(0, 1, 0, 0.3f);
                    RecommendedColorLookupTable[3].ColorAlpha = new Color(0, 0, 1, 0.3f);
                    RecommendedColorLookupTable[4].ColorAlpha = new Color(1, 0.55f, 0, 0.3f);
                    return RecommendedColorLookupTable;
                case VoxelGridType.Engine:
                    RecommendedColorLookupTable[0].Density = 0f;
                    RecommendedColorLookupTable[1].Density = 0f;
                    RecommendedColorLookupTable[2].Density = 0.2f;
                    RecommendedColorLookupTable[3].Density = 0.556f;
                    RecommendedColorLookupTable[4].Density = 1.0f;
                    RecommendedColorLookupTable[0].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[1].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[2].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[3].ColorAlpha = new Color(1, 0, 0, 0.3f);
                    RecommendedColorLookupTable[4].ColorAlpha = new Color(0, 0, 1, 0.7f);
                    return RecommendedColorLookupTable;
                case VoxelGridType.Head:
                    RecommendedColorLookupTable[0].Density = 0.2f;
                    RecommendedColorLookupTable[1].Density = 0.4f;
                    RecommendedColorLookupTable[2].Density = 0.7f;
                    RecommendedColorLookupTable[3].Density = 0.9f;
                    RecommendedColorLookupTable[4].Density = 1.0f;
                    RecommendedColorLookupTable[0].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[1].ColorAlpha = new Color(0, 0, 0, 0.3f);
                    RecommendedColorLookupTable[2].ColorAlpha = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                    RecommendedColorLookupTable[3].ColorAlpha = new Color(1, 1, 1, 0.3f);
                    RecommendedColorLookupTable[4].ColorAlpha = new Color(1, 1, 1, 0.3f);
                    return RecommendedColorLookupTable;
                case VoxelGridType.Torso:
                    RecommendedColorLookupTable[0].Density = 0.2f;
                    RecommendedColorLookupTable[1].Density = 0.21f;
                    RecommendedColorLookupTable[2].Density = 0.4f;
                    RecommendedColorLookupTable[3].Density = 0.44f;
                    RecommendedColorLookupTable[4].Density = 1.0f;
                    RecommendedColorLookupTable[0].ColorAlpha = Color.clear;
                    RecommendedColorLookupTable[1].ColorAlpha = new Color(0, 0, 0, 0.2f);
                    RecommendedColorLookupTable[2].ColorAlpha = new Color(0, 0, 0, 0.2f);
                    RecommendedColorLookupTable[3].ColorAlpha = new Color(1, 1, 1, 0.3f);
                    RecommendedColorLookupTable[4].ColorAlpha = new Color(1, 1, 1, 0.3f);
                    return RecommendedColorLookupTable;
            }
            return null;
        }

        /// <summary>
        /// Set the rotation of the voxel grid to the recommended rotation
        /// </summary>
        /// <param name="type">The voxel grid type to set the rotation for</param>
        private void setReccommendedRotation(VoxelGridType type)
        {
            switch (type)
            {
                case VoxelGridType.Bucky:
                    Rotation = new Vector3(0, 0, 0);
                    break;
                case VoxelGridType.Bunny:
                    Rotation = new Vector3(90, 180, 180); // the bunny is upside down by default. so we set it upright
                    break;
                case VoxelGridType.Engine:
                    Rotation = new Vector3(0, 0, 0);
                    break;
                case VoxelGridType.Hazelnut:
                    Rotation = new Vector3(0, 0, 0);
                    break;
                case VoxelGridType.Head:
                    Rotation = new Vector3(90, 180, 180); // have the face face the camera
                    break;
                case VoxelGridType.Torso:
                    Rotation = new Vector3(270, 180, 0); // have the torso upright with its chest to the camera
                    break;
            }
        }

        /// <summary>
        /// Whether the selected voxel grid is fully loaded
        /// </summary>
        /// <returns>true if fully loaded, otherwise false</returns>
        public bool SelectedVoxelGridDoneLoading()
        {
            switch (selectedVoxelGrid)
            {
                case VoxelGridType.Bucky:
                    return buckyGrid != null;
                case VoxelGridType.Bunny:
                    return bunnyGrid != null;
                case VoxelGridType.Engine:
                    return engineGrid != null;
                case VoxelGridType.Hazelnut:
                    return hazelnutGrid != null;
                case VoxelGridType.Head:
                    return headGrid != null;
                case VoxelGridType.Torso:
                    return torsoGrid != null;
            }
            return false;
        }

        /// <summary>
        /// Set the dimensions for the selected voxel grid
        /// </summary>
        /// <param name="type">Voxel grid type to set dimensions for</param>
        private void setGridSizes(VoxelGridType type)
        {
            switch (type)
            {
                case VoxelGridType.Bucky:
                    SizeX = 32;
                    SizeY = 32;
                    SizeZ = 32;
                    return;
                case VoxelGridType.Bunny:
                    SizeX = 512;
                    SizeY = 512;
                    SizeZ = 361;
                    return;
                case VoxelGridType.Hazelnut:
                    SizeX = 256;
                    SizeY = 256;
                    SizeZ = 256;
                    return;
                case VoxelGridType.Engine:
                    SizeX = 256;
                    SizeY = 256;
                    SizeZ = 256;
                    return;
                case VoxelGridType.Head:
                    SizeX = 256;
                    SizeY = 256;
                    SizeZ = 113;
                    return;
                case VoxelGridType.Torso:
                    SizeX = 512;
                    SizeY = 512;
                    SizeZ = 189;
                    return;
            }
        }

        /// <summary>
        /// Try to load a voxel grid using the saved data
        /// </summary>
        /// <param name="type">The voxel grid type to load the grid for</param>
        /// <returns>true if successfully loaded with saved data, otherwise false</returns>
        private bool loadKnownGrid(VoxelGridType type)
        {
            // If we have loaded this grid before we used the remembered grid
            switch (type)
            {
                case VoxelGridType.Bucky:
                    if (buckyGrid != null)
                    {
                        Grid = buckyGrid;
                        if (rayCasterManager != null)
                            rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
                        return true;
                    }
                    break;
                case VoxelGridType.Bunny:
                    if (bunnyGrid != null)
                    {
                        Grid = bunnyGrid;
                        if (rayCasterManager != null)
                            rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
                        return true;
                    }
                    break;
                case VoxelGridType.Engine:
                    if (engineGrid != null)
                    {
                        Grid = engineGrid;
                        if (rayCasterManager != null)
                            rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
                        return true;
                    }
                    break;
                case VoxelGridType.Hazelnut:
                    if (hazelnutGrid != null)
                    {
                        Grid = hazelnutGrid;
                        if (rayCasterManager != null)
                            rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
                        return true;
                    }
                    break;
                case VoxelGridType.Head:
                    if (headGrid != null)
                    {
                        Grid = headGrid;
                        if (rayCasterManager != null)
                            rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
                        return true;
                    }
                    break;
                case VoxelGridType.Torso:
                    if (torsoGrid != null)
                    {
                        Grid = torsoGrid;
                        if (rayCasterManager != null)
                            rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Load a voxel grid for the first time from the raw file
        /// </summary>
        /// <param name="type">The voxel grid type to load</param>
        private IEnumerator loadNewGrid(VoxelGridType type)
        {
            string fileName = "";
            switch (type)
            {
                case VoxelGridType.Bucky:
                    fileName = "bucky32x32x32.raw";
                    break;
                case VoxelGridType.Bunny:
                    fileName = "bunny512x512x361.raw";
                    break;
                case VoxelGridType.Hazelnut:
                    fileName = "hnut256_uint.raw";
                    break;
                case VoxelGridType.Engine:
                    fileName = "engine256x256x256.raw";
                    break;
                case VoxelGridType.Head:
                    fileName = "cthead256x256x113.raw";
                    break;
                case VoxelGridType.Torso:
                    fileName = "torso512x512x189.raw";
                    break;
            }

            string fullPath = Path.Combine(Application.streamingAssetsPath, fileName);
            BinaryReader binReader;
#if UNITY_WEBGL && !UNITY_EDITOR
            using (UnityWebRequest www = UnityWebRequest.Get(fullPath))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to load voxel grid: " + www.error);
                    yield break;
                }
                byte[] data = www.downloadHandler.data;
                MemoryStream mem = new MemoryStream(data);
                binReader = new BinaryReader(mem);
            }
#else
            if (!File.Exists(fullPath))
            {
                Debug.LogError("Voxel file not found at: " + fullPath);
                yield break;
            }
            binReader = new BinaryReader(File.Open(fullPath, FileMode.Open));
#endif

            Grid = new float[SizeX, SizeY, SizeZ];
            float minValue = 1;
            float maxValue = 0;
            for (int z = 0; z < SizeZ; z++)
            {
                int percentage = (int)Math.Round(((float)z) / SizeZ * 100f);
                loadVoxelGridProgressFill.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(progressBar.GetComponent<RectTransform>().rect.width / 100 * percentage, 0);
                loadVoxelGridtaskProgress.text = percentage + "%";
                yield return null; // allow UI update

                for (int y = 0; y < SizeY; y++)
                {
                    for (int x = 0; x < SizeX; x++)
                    {
                        switch (type)
                        {
                            case VoxelGridType.Bucky:// 8 bit raw files
                            case VoxelGridType.Bunny:
                            case VoxelGridType.Engine:
                            case VoxelGridType.Hazelnut: 
                                Grid[x, y, z] = binReader.ReadByte();
                                Grid[x, y, z] /= 255;
                                break;
                            case VoxelGridType.Torso:// 16 bit raw file
                                byte msb = binReader.ReadByte();  // most significant byte (big-endian)
                                byte lsb = binReader.ReadByte();  // least significant byte
                                ushort value = (ushort)((msb << 8) | lsb);
                                Grid[x, y, z] = value / 65535f;
                                break;
                            case VoxelGridType.Head:
                                ushort raw = binReader.ReadUInt16();
                                Grid[x, y, z] = raw / 65535f;
                                break;
                        }
                        // switch(type){
                        //     case VoxelGridType.Torso:// 16 bit raw file
                        //         Grid[x, y, z] = Grid[x, y, z] * 25;
                        //         break;
                        //     case VoxelGridType.Head:
                        //         Grid[x, y, z] = Grid[x, y, z] * 20;
                        //         break;
                        // }
                        if (Grid[x, y, z] > maxValue){
                        maxValue = Grid[x, y, z];
                        }
                        if (Grid[x, y, z] < minValue){
                            minValue = Grid[x, y, z];
                        }

                        if (Grid[x, y, z] > 1.0f){
                            Grid[x, y, z] = 1.0f;
                        }
                    }
                }
            }

            switch (type)
            {
                case VoxelGridType.Bucky:
                    buckyGrid = Grid;
                    break;
                case VoxelGridType.Bunny:
                    bunnyGrid = Grid;
                    break;
                case VoxelGridType.Engine:
                    engineGrid = Grid;
                    break;
                case VoxelGridType.Hazelnut:
                    hazelnutGrid = Grid;
                    break;
                case VoxelGridType.Head:
                    headGrid = Grid;
                    break;
                case VoxelGridType.Torso:
                    torsoGrid = Grid;
                    break;
            }

            yield return null;
        }

        /// <summary>
        /// Load a voxel grids data
        /// </summary>
        /// <param name="type">Voxel grid to load data for</param>
        private IEnumerator loadGrid(VoxelGridType type)
        {
            if (!loadKnownGrid(type))
            {
                yield return loadNewGrid(type);
            }
            //change material texture accordingly
            Renderer ren = this.GetComponent<Renderer>();
            Texture tex;
            switch (type)
            {
                //set path of voxel grid and 3d texture for preview
                case VoxelGridType.Bucky:
                    tex = Resources.Load<Texture>("3DTex/BuckyTex");
                    ren.material.SetTexture("_MainTex", tex);
                    break;
                case VoxelGridType.Bunny:
                    tex = Resources.Load<Texture>("3DTex/BunnyTex");
                    ren.material.SetTexture("_MainTex", tex);
                    break;
                case VoxelGridType.Hazelnut:
                    tex = Resources.Load<Texture>("3DTex/HazelTex");
                    ren.material.SetTexture("_MainTex", tex);
                    break;
                case VoxelGridType.Engine:
                    tex = Resources.Load<Texture>("3DTex/EngineTex");
                    ren.material.SetTexture("_MainTex", tex);
                    break;
                case VoxelGridType.Head:
                    tex = Resources.Load<Texture>("3DTex/HeadTex");
                    ren.material.SetTexture("_MainTex", tex);
                    break;
                case VoxelGridType.Torso:
                    tex = Resources.Load<Texture>("3DTex/TorsoTex");
                    ren.material.SetTexture("_MainTex", tex);
                    break;
            }
        }

        /// <summary>
        /// Set a new voxel grid type
        /// </summary>
        /// <param name="type">The new voxel grid type</param>
        public IEnumerator setVoxelGrid(VoxelGridType type)
        {

            setGridSizes(type);
            yield return loadGrid(type);
            RecommendedColorLookupTable = getReccomendedColorLookupTable(type);
            setReccommendedRotation(type);
            if (rayCasterManager != null)
                rayCasterManager.ColorLookupTable = RecommendedColorLookupTable;
        }

        /// <summary>
        /// The position of the visualized box's mesh
        /// </summary>
        public Vector3 Position
        {
            get => transform.position;
            set
            {
                if (value == transform.position) return;
                transform.position = value;
            }
        }

        /// <summary>
        /// The rotation of the visualized box's mesh
        /// </summary>
        public Vector3 Rotation
        {
            get => transform.eulerAngles;
            set
            {
                if (value == transform.eulerAngles) return;
                transform.eulerAngles = value;
            }
        }

        /// <summary>
        /// The scale of the visualized box's mesh
        /// </summary>
        public Vector3 Scale
        {
            get => transform.localScale;
            set
            {
                if (value == transform.localScale) return;
                transform.localScale = value;
            }
        }

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            rayCasterManager = RayCasterManager.Get();
        }

        public static VoxelGrid Get()
        {
            return instance;
        }
    }
}

