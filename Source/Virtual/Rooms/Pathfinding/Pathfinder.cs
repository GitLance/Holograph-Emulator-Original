using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Holo.Virtual.Rooms.Pathfinding
{
    public class Pathfinder
    {
        private virtualRoom.squareState[,] stateMap;
        private byte[,] heightMap;
        private bool[,] unitMap;
        private int maxX;
        private int maxY;

        private Heap openList;
        private Heap closedList;
        private mapNode startNode;
        private mapNode goalNode;
        private ArrayList solutionList;
        private ArrayList successorList;

        public Pathfinder(virtualRoom.squareState[,] stateMap, byte[,] heightMap, bool[,] unitMap)
        {
            this.stateMap = stateMap;
            this.heightMap = heightMap;
            this.unitMap = unitMap;

            maxX = unitMap.GetLength(1) - 1;
            maxY = unitMap.GetLength(0) - 1;

            openList = new Heap();
            closedList = new Heap();
            solutionList = new ArrayList();
            successorList = new ArrayList();
        }
        public int[] getNext(int X, int Y, int goalX, int goalY)
        {
            if (X == goalX && Y == goalY)
                return null;

            int maxCycles = (maxX * maxY);
            int Cycles = 0;

            goalNode = new mapNode(goalX, goalY, 0, null, null, this);
            startNode = new mapNode(X, Y, 0, null, goalNode, this);

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                if (Cycles == maxCycles)
                    return null;
                Cycles++;

                mapNode curNode = (mapNode)openList.Pop();
                if (curNode.sameCoordsAs(goalNode))
                {
                    while (curNode != null)
                    {
                        solutionList.Insert(0, curNode);
                        curNode = curNode.parentNode;
                        Out.WriteTrace("Contacting parent nodes");
                    }
                    break;
                }
                else
                {
                    curNode.getSuccessors();
                    successorList = curNode.allSuccessors;
                    foreach (mapNode Node in successorList)
                    {
                        mapNode nodeOpen = null;
                        if (openList.Contains(Node))
                        {
                            nodeOpen = (mapNode)openList[openList.IndexOf(Node)];
                            if (Node.totalCost > nodeOpen.totalCost)
                                continue;
                        }

                        mapNode nodeClosed = null;
                        if (closedList.Contains(Node))
                        {
                            nodeClosed = (mapNode)closedList[closedList.IndexOf(Node)];
                            if (Node.totalCost > nodeClosed.totalCost)
                                continue;
                        }

                        openList.Remove(nodeOpen);
                        closedList.Remove(nodeClosed);
                        openList.Push(Node);
                    }
                    closedList.Add(curNode);
                }
                Out.WriteTrace("Pathfinding node check cycle");
            }
            if (solutionList.Count == 0)
                return null;

            mapNode solutionNode = (mapNode)solutionList[1];
            int[] Next = new int[2];
            Next[0] = solutionNode.X;
            Next[1] = solutionNode.Y;

            return Next;
        }

        private bool spotOpen(int X, int Y)
        {
            try
            {
                if (unitMap[X, Y])
                    return false;
                if (stateMap[X, Y] != virtualRoom.squareState.Open && stateMap[X, Y] != virtualRoom.squareState.Rug)
                    return false;
                return true;
            }
            catch { return false; }
        }
        private bool spotMoveHeightOK(int X, int Y, int goalX, int goalY)
        {
            try
            {
                int oldHeight = heightMap[X, Y];
                int newHeight = heightMap[goalX, goalY];

                for (int i = -1; i <= 1; i++)
                {
                    if (oldHeight == newHeight - i)
                        return true;
                }
                return false;
            }
            catch { return false; }
        }
        private class mapNode : IComparable
        {
            internal int X;
            internal int Y;
            internal mapNode parentNode;
            private mapNode goalNode;
            private Pathfinder _Pathfinder;
            private double _Cost;
            private double _GoalEstimate;
            private ArrayList Successors;
            internal mapNode(int X, int Y, double Cost, mapNode parentNode, mapNode goalNode, Pathfinder parentPathfinder)
            {
                this.X = X;
                this.Y = Y;
                this.parentNode = parentNode;
                this.goalNode = goalNode;
                _Cost = Cost;
                _Pathfinder = parentPathfinder;
                Successors = new ArrayList();
            }
            internal bool sameCoordsAs(mapNode Node)
            {
                return (X == Node.X && Y == Node.Y);
            }
            internal ArrayList allSuccessors
            {
                get { return Successors; }
            }
            internal double Cost
            {
                get { return _Cost; }
                set { _Cost = value; }
            }
            internal double goalEstimate
            {
                get
                {
                    if (goalNode == null)
                        _GoalEstimate = 0;
                    else
                    {
                        double xD = X - goalNode.X;
                        double yD = Y - goalNode.Y;
                        _GoalEstimate = Math.Sqrt(Math.Pow(xD,2) + Math.Pow(yD,2));
                    }

                    return _GoalEstimate;
                }
                set { _GoalEstimate = value; }
            }
            internal double totalCost
            {
                get { return _Cost + goalEstimate; }
            }
            int IComparable.CompareTo(Object obj) //IComparable.CompareTo;
            {
                return -totalCost.CompareTo(((mapNode)obj).totalCost);
            }
            private void addSuccessor(int X, int Y)
            {
                if (_Pathfinder.spotOpen(X, Y) == false || _Pathfinder.spotMoveHeightOK(this.X,this.Y,X,Y) == false)
                    return;

                mapNode newNode = new mapNode(X, Y, _Cost, this, goalNode, _Pathfinder);
                Successors.Add(newNode);
                return;
            }
            internal void getSuccessors()
            {
                addSuccessor(X - 1, Y);
                addSuccessor(X + 1, Y);
                addSuccessor(X, Y - 1);
                addSuccessor(X, Y + 1);
                
                if(_Pathfinder.spotOpen(X, Y - 1))
                {
                    if(_Pathfinder.spotOpen(X - 1, Y))
                        addSuccessor(X - 1, Y - 1);
                    if(_Pathfinder.spotOpen(X + 1, Y))
                        addSuccessor(X + 1, Y - 1);
                }
                
                if(_Pathfinder.spotOpen(X + 1, Y) && _Pathfinder.spotOpen(X, Y + 1))
                    addSuccessor(X + 1, Y + 1);
                if(_Pathfinder.spotOpen(X - 1, Y) && _Pathfinder.spotOpen(X, Y - 1))
                    addSuccessor(X - 1, Y + 1);
            }
        }
    }
}
