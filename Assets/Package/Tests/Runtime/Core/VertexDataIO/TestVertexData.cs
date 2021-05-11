﻿using UnityEngine;
using NaughtyAttributes;

namespace FinTOKMAK.GridSystem.Core.Test
{
    public class TestVertexData : MonoBehaviour
    {
        [Button("Test the constructor of VertexData")]
        private void TestVertexDataConstructor()
        {
            // create two vertices and connect them
            Vertex<string> vertex1 = new Vertex<string>(new GridCoordinate(0, 0));
            Vertex<string> vertex2 = new Vertex<string>(new GridCoordinate(1, 0));
            vertex1.AddConnectionDir("right");
            vertex2.AddConnectionDir("left");
            vertex1.SetDoubleConnection("right", vertex2, "left", 10);
            
            Debug.Log("Vertex 1:");
            Debug.Log(vertex1);
            Debug.Log("Vertex 2:");
            Debug.Log(vertex2);

            VertexData<string> vertexData = new VertexData<string>(vertex1);
            
            Debug.Log("VertexData: " + vertexData);
        }
    }
}