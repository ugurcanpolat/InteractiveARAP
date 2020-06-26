using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjImporter : MonoBehaviour
{

    public Mesh targetMesh;
    // Start is called before the first frame update
    void Start()
    {

        /// <summary>
        /// Imports a mesh file, which is assigned to the main GameObject
        /// 
        /// </summary>



        /// <remarks>
        /// Hardcoded path to be replaced wtih the input value, taken from the user
        /// 
        /// </remarks>
        targetMesh = FastObjImporter.Instance.ImportFile("./Assets/armadillo_1k.obj");

        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = targetMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
