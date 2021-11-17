using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxialHexFindPath
{
    public class Step
    {
        public Step Parent;

        public float g;             //离终点的距离
        public float h;             //离起点的距离
        public float f;            // 代价
        public AxialHex Self;
        public Step(AxialHex self, AxialHex start, AxialHex end, Step parent = null)
        {
            Parent = parent;
            Self = self;
            g = self.Distance(end);

            if (parent == null)
            {
                h = self.Distance(start);
            }
            else
            {
                h = parent.h + 1;
            }
            f = g + h;
        }
    }

    /// <summary>
    /// BFS广度优先搜索
    /// </summary>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static List<AxialHex> BFS_FindPath(AxialHex start, AxialHex target)
    {

        Queue<AxialHex> frontier = new Queue<AxialHex>();
        frontier.Enqueue(start);
        Dictionary<AxialHex, AxialHex> cameFrom = new Dictionary<AxialHex, AxialHex>();
        cameFrom[start] = start;
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == target)
                break;

            for (int i = 0; i < 6; i++)
            {
                var next = current.getNeighbor(i);
                if (!TerrianManager._hexDic.ContainsKey(next) || TerrianManager._hexDic[next].Obstacle == true || cameFrom.ContainsKey(next))
                    continue;
                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }
        return getReults(cameFrom, start, target);
    }

    #region A星博客new


    public static List<AxialHex> AStarFind(AxialHex start, AxialHex target)
    {
        List<Step> forntier = new List<Step>();  //优先队列                                
        forntier.Add(new Step(start, start, target));
        Dictionary<AxialHex, AxialHex> came_from = new Dictionary<AxialHex, AxialHex>();
        came_from[start] = start;
        Dictionary<AxialHex, uint> cost_so_far = new Dictionary<AxialHex, uint>();
        cost_so_far[start] = 0;
        while (forntier.Count > 0)
        {
            forntier.Sort((a, b) => a.f.CompareTo(b.f));
            var current = forntier[0];
            forntier.Remove(current);
            if (current.Self == target)
                break;
            for (int i = 0; i < 6; i++)                         //遍历周围所有的点
            {
                var next = current.Self.getNeighbor(i);
                if (!TerrianManager._hexDic.ContainsKey(next) || TerrianManager._hexDic[next].Obstacle == true)
                    continue;
                var next_cost = cost_so_far[current.Self] + 1;
                if (!cost_so_far.ContainsKey(next) || next_cost < cost_so_far[next])
                {
                    cost_so_far[next] = next_cost;
                    var new_step = new Step(next, start, target, current);
                    forntier.Add(new_step);
                    came_from[next] = current.Self;
                }
            }
        }

        return getReults(came_from, start, target);
    }
    #endregion

    private static List<AxialHex> getReults(Dictionary<AxialHex, AxialHex> came_from, AxialHex start, AxialHex target)
    {
        List<AxialHex> result = new List<AxialHex>();
        if (came_from.ContainsKey(target))
        {
            var current = target;
            result.Add(target);
            while (came_from[current] != start)
            {
                current = came_from[current];
                result.Add(current);
            }
            result.Add(start);
        }
        return result;
    }




    public static ulong combineIndex(uint a, uint b) { return (ulong)(((ulong)a << 32) | b); }
}
