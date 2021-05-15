﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FinTOKMAK.PriorityQueue;

namespace FinTOKMAK.GridSystem.Square
{
    public class SquareGridSystem<DataType> : IGridSystem<DataType> where DataType : GridDataContainer
    {
        #region Singleton

        /// <summary>
        /// The singleton of the SquareGridSystem
        /// </summary>
        private static Dictionary<int, SquareGridSystem<DataType>> _instances = 
            new Dictionary<int, SquareGridSystem<DataType>>();

        public static Dictionary<int, SquareGridSystem<DataType>> Instances
        {
            get
            {
                return _instances;
            }
        }

        #endregion

        #region Public Field

        public List<Vertex<DataType>> vertices
        {
            get
            {
                return _vertices.Values.Cast<Vertex<DataType>>().ToList();
            }
        }

        public GridCoordinate globalCoordinateOffset
        {
            get
            {
                return _globalCoordinateOffset;
            }
            set
            {
                _globalCoordinateOffset = value;
            }
        }

        public int gridSystemID
        {
            get
            {
                return _gridSystemID;
            }
        }

        #endregion

        #region Private Field

        // a dictionary that stores all the Coordinate-Vertex pairs
        private Dictionary<GridCoordinate, SquareGridVertex<DataType>> _vertices;

        /// <summary>
        /// The global offset of all the Vertices in the grid system
        /// </summary>
        private GridCoordinate _globalCoordinateOffset;

        /// <summary>
        /// the unique ID of current grid system
        /// </summary>
        private int _gridSystemID;

        #region Pathfinding

        /// <summary>
        /// The openQueue stores all the available nodes in a PriorityQueue
        /// the priority of PathFindingVertex should be -fCost
        /// because the larger number means higher priority
        /// </summary>
        private PriorityQueue<PathFindingVertex> openQueue = new PriorityQueue<PathFindingVertex>();

        /// <summary>
        /// closeList stores all the history nodes in a dictionary
        /// Key is the coordinate of the Vertex
        /// Value is the Vertex
        /// </summary>
        private Dictionary<GridCoordinate, Vertex<DataType>> closeList =
            new Dictionary<GridCoordinate, Vertex<DataType>>();

        #endregion
        
        #endregion

        #region Consttuctor

        /// <summary>
        /// The default constructor of SquareGridSystem
        /// <param name="ID">the unique ID of the square grid system, can be the id of the generator</param>
        /// <param name="globalOffset">the global offset of all the Vertex in the grid system</param>
        /// </summary>
        public SquareGridSystem(int ID, GridCoordinate globalOffset)
        {
            // initialize the ID and offset
            _gridSystemID = ID;
            _globalCoordinateOffset = globalOffset;
            
            // initialize vertices as empty dic
            _vertices = new Dictionary<GridCoordinate, SquareGridVertex<DataType>>();
            
            // add the current GridSystem into the singleton
            if (!_instances.Values.Contains(this))
            {
                _instances.Add(_gridSystemID, this);
            }
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// The internal wrapper class for a pathfinding Vertex
        /// </summary>
        private class PathFindingVertex
        {
            /// <summary>
            /// The constructor of PathFindingVertex class
            /// the G, H, and F cost will be initialized as 0
            /// </summary>
            /// <param name="vertex">the wrapped Vertex in current PathFindingVertex class</param>
            public PathFindingVertex(Vertex<DataType> vertex)
            {
                this.vertex = vertex;
                hCost = 0;
                gCost = 0;
            }

            /// <summary>
            /// The three parameter constructor of PathFindingVertex wrapper class
            /// </summary>
            /// <param name="vertex">the wrapped Vertex in current PathFindingVertex class</param>
            /// <param name="hCost">the H cost of current Vertex</param>
            /// <param name="gCost">the G cost of current Vertex</param>
            public PathFindingVertex(Vertex<DataType> vertex, float hCost, float gCost)
            {
                this.vertex = vertex;
                this.hCost = hCost;
                this.gCost = gCost;
            }
            
            /// <summary>
            /// The current Vertex stored in this internal wrapper class
            /// </summary>
            public Vertex<DataType> vertex;

            private float m_hCost;
            /// <summary>
            /// The cost from current Vertex to the end Vertex
            /// </summary>
            public float hCost
            {
                get
                {
                    return m_hCost;
                }
                set
                {
                    m_hCost = value;
                    m_fCost = m_hCost + m_gCost;
                }
            }

            private float m_gCost;
            /// <summary>
            /// The cost from the start Vertex to current Vertex
            /// </summary>
            public float gCost
            {
                get
                {
                    return m_gCost;
                }
                set
                {
                    m_gCost = value;
                    m_fCost = m_hCost + m_gCost;
                }
            }

            private float m_fCost;

            /// <summary>
            /// The sum of _hCost and _gCost
            /// </summary>
            public float fCost
            {
                get
                {
                    return m_fCost;
                }
            }

            /// <summary>
            /// The path from the start Vertex to current Vertex
            /// start Vertex is the first Vertex in the LinkedList
            /// current Vertex is the last Vertex in the LinkedList
            /// </summary>
            public LinkedList<Vertex<DataType>> path = new LinkedList<Vertex<DataType>>();

            /// <summary>
            /// The override Equals method that compares the two vertices instead of the wrapper class
            /// </summary>
            /// <param name="obj"></param>
            /// <returns>true if the two vertices in the wrapper class are equal</returns>
            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is PathFindingVertex))
                    return false;

                return vertex.Equals(((PathFindingVertex)obj).vertex);
            }

            /// <summary>
            /// The override of GetHashCode method that returns the hashCode of its vertex
            /// </summary>
            /// <returns>the hashCode of vertex in the wrapper class</returns>
            public override int GetHashCode()
            {
                return vertex.GetHashCode();
            }
        }

        /// <summary>
        /// The helper method to find the shortest path using A* algorithm
        /// </summary>
        /// <param name="startVertex">the start vertex of pathfinding</param>
        /// <param name="endVertex">the target vertex of pathfinding</param>
        /// <returns>the path from the start Vertex to the end Vertex.
        /// Return null when path not found</returns>
        private LinkedList<Vertex<DataType>> PathfindingHelper(Vertex<DataType> startVertex, Vertex<DataType> endVertex)
        {
            // Calculate the G cost and H cost of the start Vertex
            float currnetHCost = CalculateHCost(startVertex, endVertex);
            // the start G cost is 0
            PathFindingVertex currentVertex = new PathFindingVertex(startVertex, currnetHCost, 0);
            currentVertex.path.AddLast(startVertex);
            // push the currentVertex into the openQueue
            openQueue.Push(currentVertex, -currentVertex.fCost);
            // pop the front of the openQueue
            currentVertex = openQueue.Pop();
            // find all the possible Vertices currentVertex can get to and add them to the openQueue
            foreach (Edge<DataType> edge in currentVertex.vertex.connection.Values)
            {
                // if the Vertex is accessible, calculate the F cost
                if (edge != null)
                {
                    Vertex<DataType> to = edge.to;
                    float hCost = CalculateHCost(to, endVertex);
                    float gCost = CalculateGCostForward(currentVertex, to);
                    PathFindingVertex newVertex = new PathFindingVertex(to, hCost, gCost);
                    // deep copy of current path
                    newVertex.path = new LinkedList<Vertex<DataType>>();
                    foreach (Vertex<DataType> vertex in currentVertex.path)
                    {
                        newVertex.path.AddLast(vertex);
                    }
                    newVertex.path.AddLast(to);
                    // push the newVertex into the openQueue
                    openQueue.Push(newVertex, -newVertex.fCost);
                }
            }
            // While not get to the endVertex and openQue is not empty
            while (currentVertex.vertex != endVertex && !openQueue.isEmpty)
            {
                #region Debug Path
                
                // String pathStr = "";
                // foreach (Vertex<DataType> vertex in currentVertex.path)
                // {
                //     pathStr += vertex.coordinate + "=>";
                // }
                // Debug.Log("Vertex: " + currentVertex.vertex.coordinate + "\n" + 
                //           "F Cost: " + currentVertex.fCost+ "\n" + 
                //           "Path: " + pathStr);

                #endregion

                GridCoordinate globalCoordinate = new GridCoordinate(
                    currentVertex.vertex.coordinate.x +
                    _instances[currentVertex.vertex.gridSystemID].globalCoordinateOffset.x,
                    currentVertex.vertex.coordinate.y +
                    _instances[currentVertex.vertex.gridSystemID].globalCoordinateOffset.y);
                closeList.Add(globalCoordinate, currentVertex.vertex);
                // pop the front of the openQueue
                currentVertex = openQueue.Pop();
                // find all the possible Vertices currentVertex can get to and add them to the openQueue
                foreach (Edge<DataType> edge in currentVertex.vertex.connection.Values)
                {
                    GridCoordinate toGlobalCoordinate;
                    if (edge != null) {
                        toGlobalCoordinate = new GridCoordinate(
                            edge.to.coordinate.x +
                            _instances[edge.to.gridSystemID].globalCoordinateOffset.x,
                            edge.to.coordinate.y +
                            _instances[edge.to.gridSystemID].globalCoordinateOffset.y);
                    }
                    // if edge is null, assign toGlobalCoordinate to (0, 0) is OK
                    else
                    {
                        toGlobalCoordinate = new GridCoordinate(0, 0);
                    }
                    // if the Vertex is accessible and not in the closeList, calculate the F cost
                    if (edge != null && !closeList.ContainsKey(toGlobalCoordinate))
                    {
                        Vertex<DataType> to = edge.to;
                        float hCost = CalculateHCost(to, endVertex);
                        float gCost = CalculateGCostForward(currentVertex, to);
                        PathFindingVertex newVertex = new PathFindingVertex(to, hCost, gCost);
                        // deep copy of current path
                        newVertex.path = new LinkedList<Vertex<DataType>>();
                        foreach (Vertex<DataType> vertex in currentVertex.path)
                        {
                            newVertex.path.AddLast(vertex);
                        }
                        // add the new Vertex to the path
                        newVertex.path.AddLast(to);
                        
                        // check if the openQueue already contains newVertex. If so, update it
                        PathFindingVertex queVertex = openQueue.GetElement(newVertex);
                        if (queVertex != null && -queVertex.fCost < -newVertex.fCost)
                        {
                            // change the priority of the newly found path
                            openQueue.ChangePriority(newVertex, -newVertex.fCost);
                            // change the path of the PathFindingVertex
                            queVertex.path = newVertex.path;
                            // update the cost of newVertex
                            queVertex.gCost = newVertex.gCost;
                            queVertex.hCost = newVertex.hCost;
                        }
                        else if (queVertex == null)
                        {
                            // push the newVertex into the openQueue
                            openQueue.Push(newVertex, -newVertex.fCost);
                        }
                    }
                }
            }
            
            ClearPathfindingMemory();

            // if the current vertex is not the target vertex, path not found
            if (currentVertex.vertex != endVertex)
            {
                return null;
            }

            return currentVertex.path;
        }

        /// <summary>
        /// Calculate the G cost from start Vertex to a nearby next Vertex of currentVertex
        /// </summary>
        /// <param name="currentVertex">The internal PathfindingVertex wrapper class of current Vertex. 
        /// Should contain current h,g,and f cost.
        /// Also contain the history path from start Vertex to current Vertex</param>
        /// <param name="nextVertex">The next Vertex near the current Vertex</param>
        /// <returns>the total cost from the start Vertex to the next Vertex</returns>
        private float CalculateGCostForward(PathFindingVertex currentVertex, Vertex<DataType> nextVertex)
        {
            // the current g cost
            float currentGCost = currentVertex.gCost;
            // try get the edge from currentVertex to the nextVertex
            // if the nextVertex is not the nearby Vertex of currentVertex,
            // a exception will be raised
            float edgeCost =
                GetEdge(currentVertex.vertex.coordinate, currentVertex.vertex.gridSystemID,
                    nextVertex.coordinate, nextVertex.gridSystemID);
            // the cost in the new Vertex
            float vertexCost = nextVertex.cost;

            return currentGCost + edgeCost + vertexCost;
        }

        /// <summary>
        /// Calculate the H cost from current Vertex to the end Vertex
        /// Also called heuristic function
        /// </summary>
        /// <param name="currentVertex">the current Vertex</param>
        /// <param name="endVertex">the final end Vertex of the pathfinding</param>
        /// <returns></returns>
        private float CalculateHCost(Vertex<DataType> currentVertex, Vertex<DataType> endVertex)
        {
            GridCoordinate currentOffset = _instances[currentVertex.gridSystemID].globalCoordinateOffset;
            GridCoordinate endOffset = _instances[endVertex.gridSystemID].globalCoordinateOffset;
            
            int xDiff = Math.Abs((endVertex.coordinate.x + endOffset.x) - (currentVertex.coordinate.x + currentOffset.x));
            int yDiff = Math.Abs((endVertex.coordinate.y + endOffset.y) - (currentVertex.coordinate.y + currentOffset.y));

            float res = (float)Math.Sqrt(Math.Pow((float) xDiff, 2) + Math.Pow((float) yDiff, 2));
            return res;
        }

        /// <summary>
        /// Clear openQueue and closeList after pathfinding
        /// </summary>
        private void ClearPathfindingMemory()
        {
            openQueue = new PriorityQueue<PathFindingVertex>();
            closeList = new Dictionary<GridCoordinate, Vertex<DataType>>();
        }

        #endregion

        #region IGridSystem interface

        /// <summary>
        /// Add a new edge between two vertecies
        /// </summary>
        /// <param name="coordinate1">coordinate of one vertex</param>
        /// <param name="startGridSystemID">the ID of the grid system of the start Vertex</param>
        /// <param name="coordinate2">coordinate of another vertex</param>
        /// <param name="endGridSystemID">the ID of the grid system of the end Vertex</param>
        /// <param name="weight">the weight of the edge from the start vertex to the end vertex</param>
        public void SetDoubleEdge(GridCoordinate coordinate1, int startGridSystemID,
            GridCoordinate coordinate2, int endGridSystemID, float weight)
        {
            SetEdge(coordinate1, startGridSystemID, coordinate2, endGridSystemID, weight);
            SetEdge(coordinate2, endGridSystemID, coordinate1, startGridSystemID, weight);
        }

        /// <summary>
        /// Add a new edge from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of star vertex</param>
        /// <param name="startGridSystemID">the ID of the grid system of the start Vertex</param>
        /// <param name="end">the coordinate of end vertex</param>
        /// <param name="endGridSystemID">the ID of the grid system of the end Vertex</param>
        /// <param name="weight">the weight of the edge from the start vertex to the end vertex</param>
        public void SetEdge(GridCoordinate start, int startGridSystemID, 
            GridCoordinate end, int endGridSystemID, float weight)
        {
            // Get the start and end Vertex
            SquareGridVertex<DataType> startVertex = (SquareGridVertex<DataType>)_instances[startGridSystemID].GetVertex(start);
            SquareGridVertex<DataType> endVertex = (SquareGridVertex<DataType>)_instances[endGridSystemID].GetVertex(end);

            GridCoordinate startOffset = _instances[startGridSystemID].globalCoordinateOffset;
            GridCoordinate endOffset = _instances[endGridSystemID].globalCoordinateOffset;
            
            // end Vertex is on the right of start Vertex
            if (end.x + endOffset.x == start.x + startOffset.x + 1 && 
                end.y + endOffset.y == start.y + startOffset.y)
            {
                startVertex.SetConnection("right", endVertex, weight);
            }
            // end Vertex is on the left of start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x - 1 &&
                     end.y + endOffset.y == start.y + startOffset.y)
            {
                startVertex.SetConnection("left", endVertex, weight);
            }
            // end Vertex is on the top of start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x &&
                     end.y + endOffset.y == start.y + startOffset.y + 1)
            {
                startVertex.SetConnection("up", endVertex, weight);
            }
            // end Vertex is on the bottom of start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x &&
                     end.y + endOffset.y == start.y + startOffset.y - 1)
            {
                startVertex.SetConnection("down", endVertex, weight);
            }
            // end Vertex is on the top left of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x - 1 &&
                     end.y + endOffset.y == start.y + startOffset.y + 1)
            {
                startVertex.SetConnection("upLeft", endVertex, weight);
            }
            // end Vertex is on the top right of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x + 1 &&
                     end.y + endOffset.y == start.y + startOffset.y + 1)
            {
                startVertex.SetConnection("upRight", endVertex, weight);
            }
            // end Vertex is on the bottom left of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x - 1 &&
                     end.y + endOffset.y == start.y + startOffset.y - 1)
            {
                startVertex.SetConnection("downLeft", endVertex, weight);
            }
            // end Vertex is on the bottom right of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x + 1 &&
                     end.y + endOffset.y == start.y + startOffset.y - 1)
            {
                startVertex.SetConnection("downRight", endVertex, weight);
            }
            else
            {
                throw new ArgumentException("The start Vertex and end Vertex are not neighbor, they cannot be connected");
            }
        }

        /// <summary>
        /// Add a new vertex to the current grids
        /// </summary>
        /// <param name="coordinate">the coordinate of the new vertex</param>
        /// <param name="cost">the cost of passing specific vertex</param>
        /// <param name="data">the data stored in the vertex, suggest use GridDataContainer</param>
        /// <exception cref="ArgumentException">if coordinate already exist</exception>
        public void AddVertex(GridCoordinate coordinate, float cost, DataType data)
        {
            if (_vertices.ContainsKey(coordinate))
                throw new ArgumentException("Vertex with specific coordinate already exist in the dictionary");
            
            SquareGridVertex<DataType> vertex = new SquareGridVertex<DataType>(_gridSystemID, coordinate, cost, data);
            // add the Coordinate-Vertex pair into the _vertices dict
            _vertices.Add(coordinate, vertex);
        }

        /// <summary>
        /// Find the shortest path from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of the start vertex</param>
        /// <param name="startGridSystemID">the ID of the grid system of the start Vertex</param>
        /// <param name="end">the coordinate of the end vertex</param>
        /// <param name="endGridSystemID">the ID of the grid system of the end Vertex</param>
        /// <returns>a list of verticies that lies on the shortest path from the start vertex to the end vertex</returns>
        /// <exception cref="ArgumentNullException">if the startVertex with the start coordinate
        /// or the endVertex with the end coordinate do not exist</exception>
        public LinkedList<Vertex<DataType>> FindShortestPath(GridCoordinate start, int startGridSystemID, 
            GridCoordinate end, int endGridSystemID)
        {
            // try get the startVertex
            Vertex<DataType> startVertex = _instances[startGridSystemID].GetVertex(start);
            Vertex<DataType> endVertex = _instances[endGridSystemID].GetVertex(end);

            if (startVertex == null)
            {
                throw new ArgumentNullException("The startVertex with certain coordinate do not exist.");
            }

            if (endVertex == null)
            {
                throw new ArgumentNullException("The endVertex with certain coordinate do not exist.");
            }

            return PathfindingHelper(startVertex, endVertex);
        }
        
        /// <summary>
        /// Get the weight of the edge from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of the start vertex</param>
        /// <param name="startGridSystemID">the ID of the grid system of the start Vertex</param>
        /// <param name="end">the coordinate of the end vertex</param>
        /// <param name="endGridSystemID">the ID of the grid system of the end Vertex</param>
        /// <returns>the weight of the edge that get</returns>
        public float GetEdge(GridCoordinate start, int startGridSystemID, GridCoordinate end, int endGridSystemID)
        {
            // Get the start Vertex
            SquareGridVertex<DataType> startVertex = (SquareGridVertex<DataType>)_instances[startGridSystemID].GetVertex(start);
            Dictionary<string, Edge<DataType>> startConnections = startVertex.connection;
            Edge<DataType> edge;
            
            GridCoordinate startOffset = _instances[startGridSystemID].globalCoordinateOffset;
            GridCoordinate endOffset = _instances[endGridSystemID].globalCoordinateOffset;
            
            // end Vertex is on the right of start Vertex
            if (end.x + endOffset.x == start.x + startOffset.x + 1 && 
                end.y + endOffset.y == start.y + startOffset.y)
            {
                edge = startConnections["right"];
            }
            // end Vertex is on the left of start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x - 1 &&
                     end.y + endOffset.y == start.y + startOffset.y)
            {
                edge = startConnections["left"];
            }
            // end Vertex is on the top of start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x &&
                     end.y + endOffset.y == start.y + startOffset.y + 1)
            {
                edge = startConnections["up"];
            }
            // end Vertex is on the bottom of start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x &&
                     end.y + endOffset.y == start.y + startOffset.y - 1)
            {
                edge = startConnections["down"];
            }
            // end Vertex is on the top left of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x - 1 &&
                     end.y + endOffset.y == start.y + startOffset.y + 1)
            {
                edge = startConnections["upLeft"];
            }
            // end Vertex is on the top right of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x + 1 &&
                     end.y + endOffset.y == start.y + startOffset.y + 1)
            {
                edge = startConnections["upRight"];
            }
            // end Vertex is on the Bottom left of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x - 1 &&
                     end.y + endOffset.y == start.y + startOffset.y - 1)
            {
                edge = startConnections["downLeft"];
            }
            // end Vertex is on the Bottom right of the start Vertex
            else if (end.x + endOffset.x == start.x + startOffset.x + 1 &&
                     end.y + endOffset.y == start.y + startOffset.y - 1)
            {
                edge = startConnections["downRight"];
            }
            else
            {
                throw new ArgumentException("The start Vertex and end Vertex are not neighbor, they cannot be connected");
            }

            if (edge == null)
                throw new ArgumentException("There's no connection between start Vertex and end Vertex");

            // get the cost of the edge
            return edge.cost;
        }

        /// <summary>
        /// Get and return the Vertex with certain coordinate
        /// </summary>
        /// <param name="coordinate">the target coordinate to get</param>
        /// <returns>the vertex with specific coordinate, 
        /// null when the vertex with certain coordinate do not exist</returns>
        public Vertex<DataType> GetVertex(GridCoordinate coordinate)
        {
            // if the Vertex with certain coordinate do not exist
            if (!_vertices.ContainsKey(coordinate))
                return null;

            SquareGridVertex<DataType> res = _vertices[coordinate];
            return res;
        }

        /// <summary>
        /// Remove an edge between two verticies
        /// </summary>
        /// <param name="coordinate1">coordiante of one vertex</param>
        /// <param name="startGridSystemID">the ID of the grid system of the start Vertex</param>
        /// <param name="coordinate2">coordiante of another vertex</param>
        /// <param name="endGridSystemID">the ID of the grid system of the end Vertex</param>
        public void RemoveDoubleEdge(GridCoordinate coordinate1, int startGridSystemID, 
            GridCoordinate coordinate2, int endGridSystemID)
        {
            RemoveEdge(coordinate1, startGridSystemID, coordinate2, endGridSystemID);
            RemoveEdge(coordinate2, endGridSystemID, coordinate1, startGridSystemID);
        }

        /// <summary>
        /// Remove an edge from the start vertex to the end vertex
        /// </summary>
        /// <param name="start">the coordinate of the start vertex</param>
        /// <param name="startGridSystemID">the ID of the grid system of the start Vertex</param>
        /// <param name="end">the coordinate of the end vertex</param>
        /// <param name="endGridSystemID">the ID of the grid system of the end Vertex</param>
        public void RemoveEdge(GridCoordinate start, int startGridSystemID, 
            GridCoordinate end, int endGridSystemID)
        {
            // Get the start Vertex
            SquareGridVertex<DataType> startVertex = (SquareGridVertex<DataType>)_instances[startGridSystemID].GetVertex(start);
            Edge<DataType> edge;

            GridCoordinate globalStart =
                new GridCoordinate(start.x + _instances[startGridSystemID].globalCoordinateOffset.x,
                    start.y + _instances[startGridSystemID].globalCoordinateOffset.y);
            GridCoordinate globalEnd =
                new GridCoordinate(end.x + _instances[endGridSystemID].globalCoordinateOffset.x,
                    end.y + _instances[endGridSystemID].globalCoordinateOffset.y);
            
            // end Vertex is on the right of start Vertex
            if (globalEnd.x == globalStart.x + 1 && globalEnd.y == globalStart.y)
            {
                // remove the right connection of start
                if (startVertex.connection["right"] != null)
                    startVertex.connection["right"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the left of start Vertex
            else if (globalEnd.x == globalStart.x - 1 && globalEnd.y == globalStart.y)
            {
                if (startVertex.connection["left"] != null)
                    startVertex.connection["left"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the top of start Vertex
            else if (globalEnd.x == globalStart.x && globalEnd.y == globalStart.y + 1)
            {
                if (startVertex.connection["up"] != null)
                    startVertex.connection["up"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the bottom of start Vertex
            else if (globalEnd.x == globalStart.x && globalEnd.y == globalStart.y - 1)
            {
                if (startVertex.connection["down"] != null)
                    startVertex.connection["down"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the top left of start Vertex
            else if (globalEnd.x == globalStart.x - 1 && globalEnd.y == globalStart.y + 1)
            {
                if (startVertex.connection["upLeft"] != null)
                    startVertex.connection["upLeft"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the top right of start Vertex
            else if (globalEnd.x == globalStart.x + 1 && globalEnd.y == globalStart.y + 1)
            {
                if (startVertex.connection["upRight"] != null)
                    startVertex.connection["upRight"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the bottom left of start Vertex
            else if (globalEnd.x == globalStart.x - 1 && globalEnd.y == globalStart.y - 1)
            {
                if (startVertex.connection["downLeft"] != null)
                    startVertex.connection["downLeft"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            // end Vertex is on the bottom right of start Vertex
            else if (globalEnd.x == globalStart.x + 1 && globalEnd.y == globalStart.y - 1)
            {
                if (startVertex.connection["downRight"] != null)
                    startVertex.connection["downRight"] = null;
                else
                    throw new NullReferenceException("There's no connection between start Vertex and end Vertex.");
            }
            else
            {
                Debug.LogError("Start coordinate: " + globalStart);
                Debug.LogError("End coordinate: " + globalEnd);
                throw new ArgumentException("The start Vertex and end Vertex are not neighbor, they cannot be connected");
            }
        }

        /// <summary>
        /// Remove the vertex with certain coordinate
        /// </summary>
        /// <param name="coordinate">the target coordinate to remove</param>
        /// <exception cref="ArgumentException">if the Vertex with certain coordinate do not exist</exception>
        public void RemoveVertex(GridCoordinate coordinate)
        {
            // if the target vertex do not exist
            if (!_vertices.ContainsKey(coordinate))
                throw new ArgumentException("The Vertex with certain coordinate" + coordinate.ToString() + " do not exist.");

            // Remove the edges between target Vertex and Vertices nearby
            
            // get all the edges starting from current Vertex
            SquareGridVertex<DataType> currentVertex = (SquareGridVertex<DataType>) GetVertex(coordinate);
            
            // remove the connection with top Vertex
            GridCoordinate targetCoordinate = new GridCoordinate(coordinate.x, coordinate.y + 1);
            if (currentVertex.connection["up"] != null)
            {
                int targetID = currentVertex.connection["up"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with bottom Vertex
            targetCoordinate = new GridCoordinate(coordinate.x, coordinate.y - 1);
            if (currentVertex.connection["down"] != null)
            {
                int targetID = currentVertex.connection["down"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with left Vertex
            targetCoordinate = new GridCoordinate(coordinate.x - 1, coordinate.y);
            if (currentVertex.connection["left"] != null)
            {
                int targetID = currentVertex.connection["left"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with the right Vertex
            targetCoordinate = new GridCoordinate(coordinate.x + 1, coordinate.y);
            if (currentVertex.connection["right"] != null)
            {
                int targetID = currentVertex.connection["right"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with the top left Vertex
            targetCoordinate = new GridCoordinate(coordinate.x - 1, coordinate.y + 1);
            if (currentVertex.connection["upLeft"] != null)
            {
                int targetID = currentVertex.connection["upLeft"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with top right Vertex
            targetCoordinate = new GridCoordinate(coordinate.x + 1, coordinate.y + 1);
            if (currentVertex.connection["upRight"] != null)
            {
                int targetID = currentVertex.connection["upRight"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with bottom left Vertex
            targetCoordinate = new GridCoordinate(coordinate.x - 1, coordinate.y - 1);
            if (currentVertex.connection["downLeft"] != null)
            {
                int targetID = currentVertex.connection["downLeft"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }
            // remove the connection with bottom right Vertex
            targetCoordinate = new GridCoordinate(coordinate.x + 1, coordinate.y - 1);
            if (currentVertex.connection["downRight"] != null)
            {
                int targetID = currentVertex.connection["downRight"].to.gridSystemID;
                RemoveDoubleEdge(coordinate, _gridSystemID,
                    targetCoordinate, targetID);
            }

            // if the target exist, remove the target
            _vertices.Remove(coordinate);
        }

        #endregion
    }
}