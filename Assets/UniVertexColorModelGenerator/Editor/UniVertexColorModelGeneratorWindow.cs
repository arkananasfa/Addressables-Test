using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniVertexColorModelGenerator
{
    public class UniVertexColorModelGeneratorWindow : EditorWindow
    {
        private DefaultAsset _exportDirectory = null;
        private GameObject _combineTarget = null;
        private Material _material = null; 
        private bool _exportMesh;
        private bool IsNeedColor;
        private bool CombineWithVertexColors;
        private bool WithCompressToHalf;
        private string _name;

        private GameObject ExportObject;

        [MenuItem("Window/UniVertexColorGenerateWindow")]
        static void Open()
        {
            GetWindow<UniVertexColorModelGeneratorWindow>("UniVertexColorModelGenerator").Show();
        }

        void OnGUI()
        {
            _combineTarget = (GameObject)EditorGUILayout.ObjectField("CombineTarget", _combineTarget, typeof(GameObject), true);
            _material = (Material) EditorGUILayout.ObjectField("Vertex Color Material", _material, typeof(Material),
                true);
            _exportMesh = EditorGUILayout.Toggle("Export Mesh", _exportMesh);
            IsNeedColor = EditorGUILayout.Toggle("Need Color", IsNeedColor);
            WithCompressToHalf = EditorGUILayout.Toggle("With Compress To Half", WithCompressToHalf);
            CombineWithVertexColors = EditorGUILayout.Toggle("CombineWithVertexColors", CombineWithVertexColors);
            _exportDirectory = (DefaultAsset) EditorGUILayout.ObjectField("Export Directory", _exportDirectory, typeof(DefaultAsset), true);
            _name = EditorGUILayout.TextField("Name", _name);
            ExportObject = (GameObject)EditorGUILayout.ObjectField("Export To FBX Object", ExportObject, typeof(GameObject), true);
            if (GUILayout.Button("Generate"))
            {
                if (_combineTarget == null)
                {
                    return;
                }
                GenerateMesh();
            }

            if (GUILayout.Button("ExportToFBX"))
            {
                if (ExportObject == null)
                    return;
                ExportToFBX();
            }
            
        }

        void GenerateMesh()
        {
            var meshFilters = _combineTarget.GetComponentsInChildren<MeshFilter>();
            var combineMeshInstances = new List<CombineInstance>();


            foreach (var meshFilter in meshFilters)
            {
                var mesh = meshFilter.sharedMesh;
                var vertices = new List<Vector3>();
                var subMeshCount = meshFilter.sharedMesh.subMeshCount;
                
                mesh.GetVertices(vertices);
                Color[] colors;
                if (CombineWithVertexColors)
                    colors = meshFilter.sharedMesh.colors;
                else
                    colors = new Color[vertices.Count];

                for (var i = 0; i < subMeshCount; i++)
                {

                    var triangles = new List<int>();
                    mesh.GetTriangles(triangles, i);

                    if (IsNeedColor)
                    {
                        var materialColor = _material.color;
                        for (var j = 0; j < vertices.Count; j++)
                        {
                            colors[j] = materialColor;
                        }
                    }
                    
                    var newMesh = new Mesh
                    {
                        vertices = vertices.ToArray(), triangles = triangles.ToArray(), uv = mesh.uv,
                        normals = mesh.normals, colors = colors
                    };
                    
                    var combineInstance = new CombineInstance
                        {transform = meshFilter.transform.localToWorldMatrix, mesh = newMesh};
                    combineMeshInstances.Add(combineInstance);
                }
            }

            _combineTarget.SetActive(false);

            GenerateObject(combineMeshInstances);
        }

        void GenerateObject(List<CombineInstance> instances)
        {

            var outputObjectName = _name;
            var newObject = new GameObject(outputObjectName);

            var meshRenderer = newObject.AddComponent<MeshRenderer>();
            var meshFilter = newObject.AddComponent<MeshFilter>();

            meshRenderer.material = _material;
            var mesh = new Mesh();
            mesh.CombineMeshes(instances.ToArray());
            Unwrapping.GenerateSecondaryUVSet(mesh);

            var compressMesh = mesh;
            
            if (WithCompressToHalf)
                compressMesh = CompressMeshToHalf(mesh);
            
            meshFilter.sharedMesh = compressMesh;
            newObject.transform.parent = _combineTarget.transform.parent;

            if (!_exportMesh || _exportDirectory == null)
            {
                return;
            }

            
            ExportMesh(compressMesh, outputObjectName);
        }
        
        
        [StructLayout(LayoutKind.Sequential)]
        struct VertexData
        {
           public half4 position;
           public half4 normal;
           public Color color;
           public half2 uv0;
           public half2 uv1;
        }
        Mesh CompressMeshToHalf(Mesh mesh)
        {
            Mesh newMesh = new Mesh();
            VertexData[] meshVertexData = new VertexData[mesh.vertexCount];

            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var colors = mesh.colors;
            var uv = mesh.uv;
            var uv2 = mesh.uv2;
            
            float4 pos = new float4();
            float4 normal = new float4();
            Color color = new Color();
            float2 uv0 = new float2();
            float2 uv1 = new float2();

            half4 posH;
            half4 normalH;

            half2 uv0H;
            half2 uv1H;
            
            for (int i = 0; i < meshVertexData.Length; i++)
            {
                pos.x = vertices[i].x;
                pos.y = vertices[i].y;
                pos.z = vertices[i].z;
                pos.w = 0;

                posH = (half4)pos;
                
                normal.x = normals[i].x;
                normal.y = normals[i].y;
                normal.z = normals[i].z;
                normal.w = 0;

                normalH = (half4)normal;
                
                color = colors[i];

                uv0.x = uv[i].x;
                uv0.y = uv[i].y;

                uv0H = (half2)uv0;

                uv1.x = uv2[i].x;
                uv1.y = uv2[i].y;

                uv1H = (half2)uv1;
                
                meshVertexData[i].position = posH;
                meshVertexData[i].normal = normalH;
                meshVertexData[i].color = color;
                meshVertexData[i].uv0 = uv0H;
                meshVertexData[i].uv1 = uv1H;

            }
            
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float16, 4),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 4),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, 2),
            };
            newMesh.SetVertexBufferParams(mesh.vertexCount, layout);

            newMesh.SetVertexBufferData(meshVertexData, 0, 0, meshVertexData.Length);

            int totalIndices = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
                totalIndices += (int)mesh.GetIndexCount(i);

            newMesh.SetIndexBufferParams(totalIndices, IndexFormat.UInt16);

            var indices = new List<int>();
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var subMeshIndices = new List<int>();
                mesh.GetIndices(subMeshIndices, i);
                indices.AddRange(subMeshIndices);
            }
            
            SubMeshDescriptor[] subMeshDescriptors = new SubMeshDescriptor[1];
            subMeshDescriptors[0].indexStart = 0;
            subMeshDescriptors[0].indexCount = totalIndices;
            subMeshDescriptors[0].vertexCount = mesh.vertices.Length;
            newMesh.SetSubMeshes(subMeshDescriptors);
            ushort[] ushortIndices = indices.Select(index => (ushort)index).ToArray();
            newMesh.SetIndexBufferData(ushortIndices, 0, 0, ushortIndices.Length);
            newMesh.bounds = mesh.bounds;
            newMesh.UploadMeshData(false);
            return newMesh;
        }

        void ExportMesh(Mesh mesh, string fileName)
        {
            var exportDirectoryPath = AssetDatabase.GetAssetPath(_exportDirectory);
            
            if (Path.GetExtension(fileName) != ".fbx")
            {
                fileName += ".fbx";
            }
            var exportPath = Path.Combine(exportDirectoryPath, fileName);
            
            var tempObject = new GameObject("TempExportObject");
            var meshFilter = tempObject.AddComponent<MeshFilter>();
            var meshRenderer = tempObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = _material;
            
            ModelExporter.ExportObject(exportPath, tempObject);
            
            DestroyImmediate(tempObject);
            
        }
        
        public void ExportToFBX()
        {
            var exportDirectoryPath = AssetDatabase.GetAssetPath(_exportDirectory);
            var exportPath = Path.Combine(exportDirectoryPath, _name);
        
            ModelExporter.ExportObject(exportPath, ExportObject);

        }
    }
}
