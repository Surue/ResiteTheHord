using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class PathFinding:MonoBehaviour {

    //public Transform target;
    NavigationAI navigationGraph;

    // Use this for initialization
    void Start() {
        navigationGraph = FindObjectOfType<NavigationAI>();
    }

    public List<Vector2> GetPathFromTo(Transform from, Transform target) {
        NavigationAI.Node start = navigationGraph.GetClosestNode(from.position);
        NavigationAI.Node end = navigationGraph.GetClosestNode(target.position);

        //Get A* sorted list
        Astar(start, end);

        List<Vector2> path = new List<Vector2> {
            end.position
        };

        BuildShortestPath(path, end);

        path.Reverse();

        return path;
    }

    public List<NavigationAI.Node> GetNodesFromTo(NavigationAI.Node start, NavigationAI.Node target) {
        Astar(start, target);

        List<NavigationAI.Node> path = new List<NavigationAI.Node> {
            target
        };

        BuildShortestPath(path, target);

        path.Reverse();

        return path;
    }


    public List<Vector2Int> GetPathFromTo(NavigationAI.Node start, NavigationAI.Node target) {
        //Get A* sorted list
        Astar(start, target);

        List<Vector2Int> path = new List<Vector2Int> {
            target.positionInt
        };

        BuildShortestPath(path, target);

        path.Reverse();

        return path;
    }

    void BuildShortestPath(List<NavigationAI.Node> path, NavigationAI.Node node) {
        if(node.parent == null) {
            return;
        }

        path.Add(node);
        BuildShortestPath(path, node.parent);
    }

    void BuildShortestPath(List<Vector2Int> path, NavigationAI.Node node) {
        if(node.parent == null) {
            return;
        }

        path.Add(node.positionInt);
        BuildShortestPath(path, node.parent);
    }

    void BuildShortestPath(List<Vector2> path, NavigationAI.Node node) {
        if(node.parent == null) {
            return;
        }

        path.Add(node.position);
        BuildShortestPath(path, node.parent);
    }

    void Astar(NavigationAI.Node start, NavigationAI.Node end, bool canDig = false) {
        if(canDig) {
            foreach(NavigationAI.Node node in navigationGraph.graphFull) {
                node.Reset();
                node.SetCost(Vector2.Distance(node.position, end.position));
            }
        } else {
            foreach(NavigationAI.Node node in navigationGraph.GetGraphOnlyFreeTile()) {
                node.Reset();
                node.SetCost(Vector2.Distance(node.position, end.position));
            }

            foreach(NavigationAI.Node node in navigationGraph.graphFull) {
                node.Reset();
                node.SetCost(Vector2.Distance(node.position, end.position));
            }

            foreach(NavigationAI.Node node in navigationGraph.graphCross) {
                node.Reset();
                node.SetCost(Vector2.Distance(node.position, end.position));
            }
        }


        //Make sure start position cost == 0
        start.totalCost = 0;

        List<NavigationAI.Node> openGraph = new List<NavigationAI.Node> {
            start
        };

        do {
            openGraph = openGraph.OrderBy(x => x.totalCost + x.cost).ToList();

            NavigationAI.Node node = openGraph.First();
            openGraph.Remove(node);

            foreach(NavigationAI.Node childNode in node.neighbors.OrderBy(x => x.cost + x.totalCost)) {
                float newCost = node.totalCost + Vector2.Distance(node.position, childNode.position) + childNode.cost;
                if(childNode.visited) continue;
                //if childNode.totalCost = 0 => childNode == end OR if cost is smaller than previous one
                if(childNode.totalCost == 0 || newCost < childNode.totalCost) {
                    childNode.SetTotalCost(newCost);
                    childNode.SetParent(node);
                    if(!openGraph.Contains(childNode)) {
                        if(!canDig && !childNode.isSolid) {
                            openGraph.Add(childNode);
                        }

                        if(canDig) {
                            openGraph.Add(childNode);
                        }
                    }
                }
            }
            if(node.position == end.position) {
                return;
            }
            node.visited = true;

        } while(openGraph.Count != 0);
    }
}
