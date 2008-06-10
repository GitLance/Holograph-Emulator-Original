using System;

namespace Holo.Virtual.Rooms.Pathfinding
{
    /// <summary>
    /// Provides crappy pathfinding, hence the class name.
    /// </summary>
    public class PinocchioPathfinder
    {
        public int[] Result(int X, int Y, int goalX, int goalY)
        {
            int[] outResult = new int[3];
            outResult[0] = -1;

            if (X > goalX && Y > goalY) 
            { 
                outResult[0] = X - 1; 
                outResult[1] = Y - 1; 
                outResult[2] = 7;
            }
            else if (X < goalX && Y < goalY) 
            { 
                outResult[0] = X + 1; 
                outResult[1] = Y + 1; 
                outResult[2] = 3;
            }
            else if (X > goalX && Y < goalY) 
            { 
                outResult[0] = X - 1; 
                outResult[1] = Y + 1; 
                outResult[2] = 5; 
            }
            else if (X < goalX && Y > goalY)
            {
                outResult[0] = X + 1; 
                outResult[1] = Y - 1; 
                outResult[2] = 1; 
            }
            else if (X > goalX) 
            { 
                outResult[0] = X - 1; 
                outResult[1] = Y; 
                outResult[2] = 6; 
            }
            else if (X < goalX) 
            { 
                outResult[0] = X + 1; 
                outResult[1] = Y; 
                outResult[2] = 2; 
            }
            else if (Y < goalY) 
            { 
                outResult[0] = X; 
                outResult[1] = Y + 1; 
                outResult[2] = 4;
            }
            else if (Y > goalY) 
            {
                outResult[0] = X; 
                outResult[1] = Y - 1; 
                outResult[2] = 0; 
            }

            return outResult;
        }
    }
}
