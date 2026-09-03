using System;
using System.Collections;
using System.Drawing;

namespace TheSharpTurn
{
    [Serializable]
    public class TrafficObjectList
    {
        protected SortedList trafficObjects;

        public TrafficObjectList()
        {
            trafficObjects = new SortedList();
        }

        public int NextIndex
        {
            get
            {
                return trafficObjects.Count;
            }
        }

        public int Count
        {
            get
            {
                return trafficObjects.Count;
            }
        }

        public TrafficObject this[int index]
        {
            get
            {
                if (index < 0 || index >= trafficObjects.Count)
                    return (TrafficObject)null;

                return (TrafficObject)trafficObjects.GetByIndex(index);
            }
            set
            {
                if (index >= 0 && index <= trafficObjects.Count && value != null)
                    trafficObjects[index] = value;
            }
        }

        public void Add(TrafficObject obj)
        {
            if (obj != null)
                this[NextIndex] = obj;
        }

        public void Remove(int element)
        {
            if (element >= 0 && element < trafficObjects.Count)
            {
                for (int i = element; i < trafficObjects.Count - 1; i++)
                    trafficObjects[i] = trafficObjects[i + 1];

                trafficObjects.RemoveAt(trafficObjects.Count - 1);
            }
        }

        public void Clear()
        {
            trafficObjects.Clear();
        }

        public void DrawAll(Graphics g)
        {
            for (int i = 0; i < Count; i++)
                this[i].Draw(g);
        }

        public bool IsAreaFree(Rectangle bounds)
        {
            return IsAreaFree(bounds, null);
        }

        public bool IsAreaFree(Rectangle bounds, TrafficObject ignoredObject)
        {
            TrafficObject current;

            for (int i = 0; i < Count; i++)
            {
                current = this[i];

                if (current == ignoredObject)
                    continue;

                if (current.Bounds.IntersectsWith(bounds))
                    return false;

                Rectangle reservedBounds = GetLaneChangeReservedBounds(current);

                if (!reservedBounds.IsEmpty &&
                    reservedBounds.IntersectsWith(bounds))
                    return false;
            }

            return true;
        }

        private Rectangle GetLaneChangeReservedBounds(TrafficObject obj)
        {
            int targetY = obj.Y;
            bool isChangingLane = false;

            if (obj is RoadUser)
            {
                RoadUser roadUser = (RoadUser)obj;
                isChangingLane = roadUser.IsChangingLane;
                targetY = roadUser.LaneChangeTargetY;
            }
            else if (obj is BikePathUser)
            {
                BikePathUser bikePathUser = (BikePathUser)obj;
                isChangingLane = bikePathUser.IsChangingLane;
                targetY = bikePathUser.LaneChangeTargetY;
            }
            else if (obj is SidewalkUser)
            {
                SidewalkUser sidewalkUser = (SidewalkUser)obj;
                isChangingLane = sidewalkUser.IsChangingLane;
                targetY = sidewalkUser.LaneChangeTargetY;
            }

            if (!isChangingLane)
                return Rectangle.Empty;

            int topY = Math.Min(obj.Y, targetY);
            int bottomY = Math.Max(obj.Y + obj.Height,
                targetY + obj.Height);

            return new Rectangle(obj.X, topY, obj.Width,
                bottomY - topY);
        }

        public int CountObjectsInLane(int lane)
        {
            int count = 0;

            for (int i = 0; i < Count; i++)
            {
                if (this[i].Lane == lane)
                    count++;
            }

            return count;
        }

        public int FindObjectIndexAt(int xP, int yP)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].Contains(xP, yP))
                    return i;
            }

            return -1;
        }

        public TrafficObject FindObjectAt(int xP, int yP)
        {
            int index = FindObjectIndexAt(xP, yP);

            if (index >= 0)
                return this[index];

            return (TrafficObject)null;
        }
    }
}
