using System;
namespace Holo.Virtual.Rooms.Pathfinding
{
    public static class Rotation
    {
        public static byte Calculate(int X1, int Y1, int X2, int Y2)
        {
            byte Rotation = 0;
            if (X1 > X2 && Y1 > Y2)
                Rotation = 7;
            else if(X1 < X2 && Y1 < Y2)
                Rotation = 3;
            else if(X1 > X2 && Y1 < Y2)
                Rotation = 5;
            else if(X1 < X2 && Y1 > Y2)
                Rotation = 1;
            else if(X1 > X2)
                Rotation = 6;
            else if(X1 < X2)
                Rotation = 2;
            else if(Y1 < Y2)
                Rotation = 4;
            else if(Y1 > Y2)
                Rotation = 0;

            return Rotation;
        }
        public static byte headRotation(byte headRot, int X, int Y, int toX, int toY)
        {
            if(headRot == 2)
            {
                if(X <= toX && Y < toY)
                    return 3;
                else if(X <= toX && Y > toY)
                    return 5;
                else if(X < toX && Y == toY)
                    return 2;
            }
            else if(headRot == 4)
            {
                if(X > toX && Y <= toY)
                    return 5;
                else if(X < toX && Y <= toY)
                    return 3;
                else if(X == toX && Y < toY)
                    return 4;
            }
            else if(headRot == 6)
            {
                if(X >= toX && Y > toY)
                    return 7;
                else if(X >= toX && Y < toY)
                    return 5;
                else if(X > toX && Y == toY)
                    return 6;
            }
            else if(headRot == 0)
            {
                if(X > toX && Y >= toY)
                    return 9;
                if(X < toX && Y >= toY)
                    return 1;
                if(X == toX && Y > toY)
                    return 0;
            }
            return 10;
        }
    }
}
