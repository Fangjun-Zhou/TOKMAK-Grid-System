﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace GridSystem
{
    public interface IGridSystem<DataType>
    {
        #region Vertext Operation

        /// <summary>
        /// Add a new vertex to the current grids
        /// </summary>
        /// <param name="coordinate">the coordinate of the new vertex</param>
        /// <param name="cost">the cost of passing specific vertex</param>
        /// <param name="data">the data stored in the vertex</param>
        void AddVertex(GridCoordinate coordinate, float cost, DataType data);

        /// <summary>
        /// Remove a vertex from the current grids
        /// </summary>
        /// <param name="coordinate">the coordinate of the target vertex</param>
        /// <exception cref="ArgumentException">if the vertex with specific coordinate does not exist</exception>
        void RemoveVertex(GridCoordinate coordinate);

        /// <summary>
        /// Get a vertex with certain coordinate
        /// </summary>
        /// <param name="coordinate">the coordinate of the specific vertex to get</param>
        /// <exception cref="ArgumentException">if the vertext to remove with specific coordinate does not exist</exception>
        Vertex<DataType> GetVertex(GridCoordinate coordinate);

        #endregion

        #region Edge Operation

        /// <summary>
        /// Add a new edge from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of star vertex</param>
        /// <param name="end">the coordinate of end vertex</param>
        /// <param name="weight">the weight of the edge from the start vertex to the end vertex</param>
        void AddEdge(GridCoordinate start, GridCoordinate end, float weight);

        /// <summary>
        /// Add a new edge between two vertecies
        /// </summary>
        /// <param name="coordinate1">coordiante of one vertex</param>
        /// <param name="coordinate2">coordiante of another vertex</param>
        /// <param name="weight">the weight of the edge from the start vertex to the end vertex</param>
        void AddDoubleEdge(GridCoordinate coordinate1, GridCoordinate coordinate2, float weight);

        /// <summary>
        /// Remove an edge from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of the start vertex</param>
        /// <param name="end">the coordinate of the end vertex</param>
        void RemoveEdge(GridCoordinate start, GridCoordinate end);

        /// <summary>
        /// Remove an edge between two verticies
        /// </summary>
        /// <param name="coordinate1">coordiante of one vertex</param>
        /// <param name="coordinate2">coordiante of another vertex</param>
        void RemoveDoubleEdge(GridCoordinate coordinate1, GridCoordinate coordinate2);

        /// <summary>
        /// Get the weight of the edge from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of the start vertex</param>
        /// <param name="end">the coordinate of the end vertex</param>
        /// <returns>the weight of the edge that get</returns>
        float GetEdge(GridCoordinate start, GridCoordinate end);

        #endregion

        #region Path Finding

        /// <summary>
        /// Find the shortest path from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of the start vertex</param>
        /// <param name="end">the coordinate of the end vertex</param>
        /// <returns>a list of verticies that lies on the shortest path from the start vertex to the end vertex</returns>
        List<Vertex<DataType>> FindShortestPath(GridCoordinate start, GridCoordinate end);

        #endregion
    }
}