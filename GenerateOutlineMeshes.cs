using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEditor.Rendering;
using System.Collections.Generic;
using System.Linq;

public class GenerateOutlineMeshes : AssetPostprocessor
{

    void OnPostprocessModel(GameObject gameObject) 
    {
        if (assetImporter.assetPath.Contains("Outline")) //need some more complex conditions here
        {
            Debug.Log($"Creating outline meshes for model '{gameObject.name}'");

            //remove fbx file from filepath
            string[] assetPathchunks = assetImporter.assetPath.Split("/");
            for (int i = 0; i < assetPathchunks.Length; i++)
            {
                assetPathchunks[i] += "/";
            }

            //recombine path and add new folder name
            assetPath = String.Join("", assetPathchunks.Take(assetPathchunks.Count() - 1).ToArray());
            assetPath = String.Join("", assetPath, "GeneratedMeshes/");

            //create the folder if it doesn't exist (this just returns a reference to it if it does
            System.IO.Directory.CreateDirectory(assetPath);

            Apply(gameObject, assetPath);
        }
    } 
    void Apply(GameObject gameObject, string assetPath)
    {

        Transform ogTransform = gameObject.transform; //this is what we're parenting things under

        var meshfilters = gameObject.GetComponentsInChildren<MeshFilter>(); //submeshes essentially
        foreach (var mf in meshfilters)
        {
            Debug.Log($"Creating outline version of submesh '{mf.name}'");
            Mesh ogmesh = mf.sharedMesh; //sharedMesh is safer


            //mesh combine flow --------------

            int oldvertindex = 0; //the vert indices from the original mesh
            int newvertindex = 0; //the vert indices for the new generated mesh

            Dictionary<Vector3, int> vertcoords_to_new_index = new Dictionary<Vector3, int>(); //used to match vert indices together by coords 
            Dictionary<int, int> old_to_new_index = new Dictionary<int,int>(); 

            List<Vector3> newvertslist = new List<Vector3>(); //deduplicated vert coords 
            List<int> newtriangleslist = new List<int>();     //^

            //TODO - add bone weights to support skeletal meshes

            foreach(Vector3 vert in ogmesh.vertices)
            {

                if (!vertcoords_to_new_index.ContainsKey(vert))  //if we haven't encountered this vertex coordinate before:
                {
                    vertcoords_to_new_index[vert] = newvertindex;              //link the coord to the NEW vertex index so that we can find it later and link duplicates to it
                    old_to_new_index[oldvertindex] = newvertindex;          //link that old index to new index
                    newvertslist.Add(vert);                               //add coords to list that will eventually be used to create the new mesh
                    newvertindex++;                                         
                }
                else
                {
                    int surviving_new_index = vertcoords_to_new_index[vert];        //if the vert coords aren't new to us, we store them in the dict pointing towards
                    old_to_new_index[oldvertindex]= surviving_new_index;            // the new index of the first vert we encountered that had those coords
                }
                oldvertindex++;
            }

            foreach(int index in ogmesh.triangles)   //just convert each index to new index and store in list
            {
                int new_index = old_to_new_index[index];
                newtriangleslist.Add(new_index);
            }


            //make outline mesh
            Mesh newmesh = new Mesh();
            newmesh.vertices = newvertslist.ToArray();
            newmesh.name = ($"{ogmesh.name}_outline.mesh");
            newmesh.triangles = newtriangleslist.ToArray();
            newmesh.RecalculateNormals();


            string filepath = ($"{assetPath}{newmesh.name}");    
            AssetDatabase.CreateAsset(newmesh, filepath); //TODO : This is apparently deprecated now. Looking into alternative

            string newname = ($"{mf.gameObject.name}_Outline");

            //add new mesh game object for the mesh we've created
            var newobject = new GameObject(newname); // , typeof(MeshFilter), typeof(MeshRenderer));
            newobject.AddComponent(typeof(MeshFilter));
            newobject.AddComponent(typeof(MeshRenderer));
            newobject.GetComponent<MeshFilter>().sharedMesh = newmesh;

            //assign outline material (TO-DO: replace hard coded material path)
            Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/M_Outline.mat", typeof(Material));
            newobject.GetComponent<MeshRenderer>().sharedMaterial = mat;

            //parent under prefab root
            newobject.transform.SetParent(ogTransform, false);





        }
    }
}
    

