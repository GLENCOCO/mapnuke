﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// This class handles the creation of all conceptual parts of the world.
/// This is where province types and connection types are determined.
/// </summary>
static class WorldGenerator
{
    const int PROVS_PER_PLAYER = 16;

    static int m_num_players;
    static bool m_nat_starts = true;
    static bool m_teamplay = false;
    static bool m_cluster_water = true;
    static NodeLayout m_layout;
    static List<Node> m_nodes;
    static List<Node> m_starts;
    static List<Connection> m_connections;
    static List<NodeLayout> m_layouts;
    static List<NationData> m_nations;

    public static void GenerateWorld(bool teamplay, bool cluster_water, bool nat_starts, List<NationData> picks)
    {
        init();

        m_num_players = picks.Count;
        m_nations = picks;
        m_nat_starts = nat_starts;
        m_teamplay = teamplay;
        m_cluster_water = cluster_water;

        generate_nodes();
        generate_connections();
        generate_caprings();
        generate_seas();
        generate_lakes();

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.IsWater).ToList();

        generate_swamps(valid);
        generate_misc(valid);
        generate_cliffs();
        generate_rivers();
        generate_roads();
        generate_farms(valid);
        generate_sized(valid);
        generate_thrones();
        cleanup_connections();
    }

    public static List<Connection> GetConnections()
    {
        return m_connections;
    }

    public static List<Node> GetNodes()
    {
        return m_nodes;
    }

    public static NodeLayout GetLayout()
    {
        return m_layout;
    }

    static void init()
    {
        m_layouts = new List<NodeLayout>(); // create all the layouts for all allowed player counts. ugly but functional.

        // A layout is organized as a grid.
        // This 2 player layout is 6x6 nodes.
        // Spawn point X and Y values start at 0, it helps to draw the grid on a piece of paper to visualize it.
        // Example: The bottom left position on a 6x6 grid is [0,0]. The top left position on the grid is [5,5].
        NodeLayout n2 = new NodeLayout(2, 2, 6, 6, 18);
        n2.AddPlayer(1, 1);
        n2.AddPlayer(4, 4);
        n2.AddThrone(1, 4);
        n2.AddThrone(4, 1);

        NodeLayout n3 = new NodeLayout(3, 3, 8, 8, 21);
        n3.AddPlayer(1, 3);
        n3.AddPlayer(5, 1);
        n3.AddPlayer(5, 5);
        n3.AddThrone(3, 1);
        n3.AddThrone(3, 5);
        n3.AddThrone(5, 3);

        NodeLayout n4 = new NodeLayout(4, 4, 8, 8, 16);
        n4.AddPlayer(1, 1, 0);
        n4.AddPlayer(1, 5, 0);
        n4.AddPlayer(5, 1, 1);
        n4.AddPlayer(5, 5, 1);
        n4.AddThrone(3, 3);
        n4.AddThrone(3, 7);
        n4.AddThrone(7, 3);
        n4.AddThrone(7, 7);

        NodeLayout n5 = new NodeLayout(5, 6, 9, 9, 16);
        n5.AddPlayer(1, 1);
        n5.AddPlayer(1, 6);
        n5.AddPlayer(4, 4);
        n5.AddPlayer(7, 2);
        n5.AddPlayer(7, 7);
        n5.AddThrone(0, 3);
        n5.AddThrone(2, 3);
        n5.AddThrone(3, 7);
        n5.AddThrone(5, 1);
        n5.AddThrone(6, 5);
        n5.AddThrone(8, 5);

        NodeLayout n6 = new NodeLayout(6, 6, 12, 8, 16);
        n6.AddPlayer(1, 1, 0);
        n6.AddPlayer(1, 5, 0);
        n6.AddPlayer(5, 1, 1);
        n6.AddPlayer(5, 5, 1);
        n6.AddPlayer(9, 1, 2);
        n6.AddPlayer(9, 5, 2);
        n6.AddThrone(3, 3);
        n6.AddThrone(3, 7);
        n6.AddThrone(7, 3);
        n6.AddThrone(7, 7);
        n6.AddThrone(11, 3);
        n6.AddThrone(11, 7);

        NodeLayout n7 = new NodeLayout(7, 7, 12, 12, 20);
        n7.AddPlayer(2, 1);
        n7.AddPlayer(6, 1);
        n7.AddPlayer(10, 3);
        n7.AddPlayer(6, 5);
        n7.AddPlayer(2, 7);
        n7.AddPlayer(6, 9);
        n7.AddPlayer(10, 9);
        n7.AddThrone(0, 2);
        n7.AddThrone(0, 8);
        n7.AddThrone(4, 1);
        n7.AddThrone(4, 6);
        n7.AddThrone(8, 4);
        n7.AddThrone(6, 11);
        n7.AddThrone(8, 9);

        NodeLayout n8 = new NodeLayout(8, 8, 16, 8, 16);
        n8.AddPlayer(1, 1, 0);
        n8.AddPlayer(1, 5, 0);
        n8.AddPlayer(5, 1, 1);
        n8.AddPlayer(5, 5, 1);
        n8.AddPlayer(9, 1, 2);
        n8.AddPlayer(9, 5, 2);
        n8.AddPlayer(13, 1, 3);
        n8.AddPlayer(13, 5, 3);
        n8.AddThrone(3, 3);
        n8.AddThrone(3, 7);
        n8.AddThrone(7, 3);
        n8.AddThrone(7, 7);
        n8.AddThrone(11, 3);
        n8.AddThrone(11, 7);
        n8.AddThrone(15, 3);
        n8.AddThrone(15, 7);

        NodeLayout n9 = new NodeLayout(9, 9, 12, 12, 16);
        n9.AddPlayer(1, 1);
        n9.AddPlayer(1, 5);
        n9.AddPlayer(1, 9);
        n9.AddPlayer(5, 1);
        n9.AddPlayer(5, 5);
        n9.AddPlayer(5, 9);
        n9.AddPlayer(9, 1);
        n9.AddPlayer(9, 5);
        n9.AddPlayer(9, 9);
        n9.AddThrone(3, 3);
        n9.AddThrone(3, 7);
        n9.AddThrone(3, 11);
        n9.AddThrone(7, 3);
        n9.AddThrone(7, 7);
        n9.AddThrone(7, 11);
        n9.AddThrone(11, 3);
        n9.AddThrone(11, 7);
        n9.AddThrone(11, 11);

        NodeLayout n10 = new NodeLayout(10, 10, 15, 15, 22);
        n10.AddPlayer(2, 2, 0);
        n10.AddPlayer(6, 2, 0);
        n10.AddPlayer(11, 1, 1);
        n10.AddPlayer(13, 5, 1);
        n10.AddPlayer(9, 6, 2);
        n10.AddPlayer(5, 8, 2);
        n10.AddPlayer(1, 9, 3);
        n10.AddPlayer(3, 13, 3);
        n10.AddPlayer(8, 12, 4);
        n10.AddPlayer(12, 12, 4);
        n10.AddThrone(6, 0);
        n10.AddThrone(13, 0);
        n10.AddThrone(1, 4);
        n10.AddThrone(6, 6);
        n10.AddThrone(8, 8);
        n10.AddThrone(13, 7);
        n10.AddThrone(13, 10);
        n10.AddThrone(1, 7);
        n10.AddThrone(1, 14);
        n10.AddThrone(8, 14);

        NodeLayout n11 = new NodeLayout(11, 12, 16, 16, 23);
        n11.AddPlayer(4, 1);
        n11.AddPlayer(5, 2);
        n11.AddPlayer(9, 1);
        n11.AddPlayer(8, 7);
        n11.AddPlayer(12, 5);
        n11.AddPlayer(14, 1);
        n11.AddPlayer(4, 9);
        n11.AddPlayer(2, 13);
        n11.AddPlayer(7, 13);
        n11.AddPlayer(12, 13);
        n11.AddPlayer(14, 9);
        n11.AddThrone(2, 1);
        n11.AddThrone(7, 1);
        n11.AddThrone(6, 4);
        n11.AddThrone(10, 4);
        n11.AddThrone(15, 3);
        n11.AddThrone(4, 6);
        n11.AddThrone(1, 11);
        n11.AddThrone(6, 10);
        n11.AddThrone(12, 8);
        n11.AddThrone(10, 10);
        n11.AddThrone(9, 13);
        n11.AddThrone(14, 13);

        NodeLayout n12 = new NodeLayout(12, 12, 16, 12, 16);
        n12.AddPlayer(1, 1, 0);
        n12.AddPlayer(1, 5, 2);
        n12.AddPlayer(1, 9, 4);
        n12.AddPlayer(5, 1, 0);
        n12.AddPlayer(5, 5, 2);
        n12.AddPlayer(5, 9, 4);
        n12.AddPlayer(9, 1, 1);
        n12.AddPlayer(9, 5, 3);
        n12.AddPlayer(9, 9, 5);
        n12.AddPlayer(13, 1, 1);
        n12.AddPlayer(13, 5, 3);
        n12.AddPlayer(13, 9, 5);
        n12.AddThrone(3, 3);
        n12.AddThrone(3, 7);
        n12.AddThrone(3, 11);
        n12.AddThrone(7, 3);
        n12.AddThrone(7, 7);
        n12.AddThrone(7, 11);
        n12.AddThrone(11, 3);
        n12.AddThrone(11, 7);
        n12.AddThrone(11, 11);
        n12.AddThrone(15, 3);
        n12.AddThrone(15, 7);
        n12.AddThrone(15, 11);

        NodeLayout n13 = new NodeLayout(13, 12, 16, 16, 19);
        n13.AddPlayer(2, 1);
        n13.AddPlayer(3, 5);
        n13.AddPlayer(6, 1);
        n13.AddPlayer(8, 7);
        n13.AddPlayer(10, 1);
        n13.AddPlayer(12, 5);
        n13.AddPlayer(13, 9);
        n13.AddPlayer(14, 13);
        n13.AddPlayer(14, 1);
        n13.AddPlayer(10, 13);
        n13.AddPlayer(6, 13);
        n13.AddPlayer(2, 13);
        n13.AddPlayer(4, 9);
        n13.AddThrone(4, 1);
        n13.AddThrone(6, 4);
        n13.AddThrone(9, 4);
        n13.AddThrone(12, 1);
        n13.AddThrone(15, 6);
        n13.AddThrone(15, 4);
        n13.AddThrone(10, 10);
        n13.AddThrone(12, 13);
        n13.AddThrone(7, 10);
        n13.AddThrone(4, 13);
        n13.AddThrone(1, 10);
        n13.AddThrone(1, 8);

        NodeLayout n14 = new NodeLayout(14, 14, 18, 18, 23);
        n14.AddPlayer(4, 1, 0);
        n14.AddPlayer(7, 6, 6);
        n14.AddPlayer(8, 1, 4);
        n14.AddPlayer(11, 9, 6);
        n14.AddPlayer(12, 1, 4);
        n14.AddPlayer(14, 5, 2);
        n14.AddPlayer(16, 10, 1);
        n14.AddPlayer(16, 1, 2);
        n14.AddPlayer(14, 14, 1);
        n14.AddPlayer(2, 5, 0);
        n14.AddPlayer(4, 10, 3);
        n14.AddPlayer(2, 14, 3);
        n14.AddPlayer(6, 14, 5);
        n14.AddPlayer(10, 14, 5);
        n14.AddThrone(1, 2);
        n14.AddThrone(2, 7);
        n14.AddThrone(1, 11);
        n14.AddThrone(3, 17);
        n14.AddThrone(8, 9);
        n14.AddThrone(7, 11);
        n14.AddThrone(7, 16);
        n14.AddThrone(11, 17);
        n14.AddThrone(15, 16);
        n14.AddThrone(10, 6);
        n14.AddThrone(11, 4);
        n14.AddThrone(16, 8);
        n14.AddThrone(17, 13);
        n14.AddThrone(17, 4);

        NodeLayout n15 = new NodeLayout(15, 15, 20, 12, 16);
        n15.AddPlayer(1, 1);
        n15.AddPlayer(1, 5);
        n15.AddPlayer(1, 9);
        n15.AddPlayer(5, 1);
        n15.AddPlayer(5, 5);
        n15.AddPlayer(5, 9);
        n15.AddPlayer(9, 1);
        n15.AddPlayer(9, 5);
        n15.AddPlayer(9, 9);
        n15.AddPlayer(13, 1);
        n15.AddPlayer(13, 5);
        n15.AddPlayer(13, 9);
        n15.AddPlayer(17, 1);
        n15.AddPlayer(17, 5);
        n15.AddPlayer(17, 9);
        n15.AddThrone(3, 3);
        n15.AddThrone(3, 7);
        n15.AddThrone(3, 11);
        n15.AddThrone(7, 3);
        n15.AddThrone(7, 7);
        n15.AddThrone(7, 11);
        n15.AddThrone(11, 3);
        n15.AddThrone(11, 7);
        n15.AddThrone(11, 11);
        n15.AddThrone(15, 3);
        n15.AddThrone(15, 7);
        n15.AddThrone(15, 11);
        n15.AddThrone(19, 3);
        n15.AddThrone(19, 7);
        n15.AddThrone(19, 11);

        NodeLayout n16 = new NodeLayout(16, 16, 16, 16, 16);
        n16.AddPlayer(1, 1, 0);
        n16.AddPlayer(1, 5, 2);
        n16.AddPlayer(1, 9, 4);
        n16.AddPlayer(1, 13, 6);
        n16.AddPlayer(5, 1, 0);
        n16.AddPlayer(5, 5, 2);
        n16.AddPlayer(5, 9, 4);
        n16.AddPlayer(5, 13, 6);
        n16.AddPlayer(9, 1, 1);
        n16.AddPlayer(9, 5, 3);
        n16.AddPlayer(9, 9, 5);
        n16.AddPlayer(9, 13, 7);
        n16.AddPlayer(13, 1, 1);
        n16.AddPlayer(13, 5, 3);
        n16.AddPlayer(13, 9, 5);
        n16.AddPlayer(13, 13, 7);
        n16.AddThrone(3, 3);
        n16.AddThrone(3, 7);
        n16.AddThrone(3, 11);
        n16.AddThrone(3, 15);
        n16.AddThrone(7, 3);
        n16.AddThrone(7, 7);
        n16.AddThrone(7, 11);
        n16.AddThrone(7, 15);
        n16.AddThrone(11, 3);
        n16.AddThrone(11, 7);
        n16.AddThrone(11, 11);
        n16.AddThrone(11, 15);
        n16.AddThrone(15, 3);
        n16.AddThrone(15, 7);
        n16.AddThrone(15, 11);
        n16.AddThrone(15, 15);

        NodeLayout n17 = new NodeLayout(17, 16, 20, 20, 23);
        n17.AddPlayer(2, 3);
        n17.AddPlayer(6, 5);
        n17.AddPlayer(6, 1);
        n17.AddPlayer(10, 3);
        n17.AddPlayer(10, 9);
        n17.AddPlayer(14, 13);
        n17.AddPlayer(14, 6);
        n17.AddPlayer(14, 1);
        n17.AddPlayer(18, 15);
        n17.AddPlayer(18, 8);
        n17.AddPlayer(18, 2);
        n17.AddPlayer(2, 10);
        n17.AddPlayer(6, 12);
        n17.AddPlayer(10, 15);
        n17.AddPlayer(14, 17);
        n17.AddPlayer(6, 17);
        n17.AddPlayer(2, 16);
        n17.AddThrone(3, 0);
        n17.AddThrone(9, 0);
        n17.AddThrone(10, 6);
        n17.AddThrone(14, 11);
        n17.AddThrone(14, 8);
        n17.AddThrone(14, 3);
        n17.AddThrone(18, 11);
        n17.AddThrone(19, 5);
        n17.AddThrone(2, 7);
        n17.AddThrone(6, 7);
        n17.AddThrone(6, 10);
        n17.AddThrone(6, 15);
        n17.AddThrone(1, 13);
        n17.AddThrone(10, 12);
        n17.AddThrone(11, 18);
        n17.AddThrone(17, 17);

        NodeLayout n18 = new NodeLayout(18, 18, 20, 20, 22);
        n18.AddPlayer(2, 1, 0);
        n18.AddPlayer(6, 1, 0);
        n18.AddPlayer(10, 2, 1);
        n18.AddPlayer(14, 2, 1);
        n18.AddPlayer(18, 1, 2); 
        n18.AddPlayer(7, 6, 3);
        n18.AddPlayer(12, 7, 4);
        n18.AddPlayer(13, 11, 5);
        n18.AddPlayer(18, 6, 2);
        n18.AddPlayer(18, 11, 5);
        n18.AddPlayer(18, 16, 6);
        n18.AddPlayer(2, 6, 3);
        n18.AddPlayer(2, 11, 8);
        n18.AddPlayer(2, 16, 8);
        n18.AddPlayer(8, 10, 4);
        n18.AddPlayer(6, 15, 7);
        n18.AddPlayer(10, 15, 7);
        n18.AddPlayer(14, 16, 6);
        n18.AddThrone(5, 4);
        n18.AddThrone(10, 4);
        n18.AddThrone(15, 13);
        n18.AddThrone(15, 8);
        n18.AddThrone(15, 5);
        n18.AddThrone(19, 9);
        n18.AddThrone(19, 18);
        n18.AddThrone(18, 3);
        n18.AddThrone(1, 8);
        n18.AddThrone(5, 9);
        n18.AddThrone(5, 12);
        n18.AddThrone(10, 13);
        n18.AddThrone(2, 14);
        n18.AddThrone(1, 19);
        n18.AddThrone(5, 18);
        n18.AddThrone(8, 18);
        n18.AddThrone(12, 19);
        n18.AddThrone(15, 19);

        NodeLayout n19 = new NodeLayout(19, 19, 20, 20, 21);
        n19.AddPlayer(2, 2);
        n19.AddPlayer(6, 2);
        n19.AddPlayer(10, 1);
        n19.AddPlayer(10, 9);
        n19.AddPlayer(12, 5);
        n19.AddPlayer(14, 11);
        n19.AddPlayer(14, 1);
        n19.AddPlayer(17, 6);
        n19.AddPlayer(18, 16);
        n19.AddPlayer(18, 11);
        n19.AddPlayer(18, 1);
        n19.AddPlayer(2, 7);
        n19.AddPlayer(6, 7);
        n19.AddPlayer(3, 12);
        n19.AddPlayer(8, 13);
        n19.AddPlayer(2, 17);
        n19.AddPlayer(6, 17);
        n19.AddPlayer(10, 17);
        n19.AddPlayer(14, 16);
        n19.AddThrone(5, 4);
        n19.AddThrone(9, 6);
        n19.AddThrone(9, 3);
        n19.AddThrone(14, 8);
        n19.AddThrone(15, 14);
        n19.AddThrone(15, 4);
        n19.AddThrone(18, 9);
        n19.AddThrone(19, 14);
        n19.AddThrone(1, 4);
        n19.AddThrone(2, 9);
        n19.AddThrone(6, 10);
        n19.AddThrone(5, 14);
        n19.AddThrone(11, 12);
        n19.AddThrone(11, 15);
        n19.AddThrone(2, 19);
        n19.AddThrone(7, 19);
        n19.AddThrone(13, 19);
        n19.AddThrone(18, 19);

        NodeLayout n20 = new NodeLayout(20, 20, 20, 16, 16);
        n20.AddPlayer(1, 1, 0);
        n20.AddPlayer(1, 5, 0);
        n20.AddPlayer(1, 9, 1);
        n20.AddPlayer(1, 13, 1);
        n20.AddPlayer(5, 1, 2);
        n20.AddPlayer(5, 5, 2);
        n20.AddPlayer(5, 9, 3);
        n20.AddPlayer(5, 13, 3);
        n20.AddPlayer(9, 1, 4);
        n20.AddPlayer(9, 5, 4);
        n20.AddPlayer(9, 9, 5);
        n20.AddPlayer(9, 13, 5);
        n20.AddPlayer(13, 1, 6);
        n20.AddPlayer(13, 5, 6);
        n20.AddPlayer(13, 9, 7);
        n20.AddPlayer(13, 13, 7);
        n20.AddPlayer(17, 1, 8);
        n20.AddPlayer(17, 5, 8);
        n20.AddPlayer(17, 9, 9);
        n20.AddPlayer(17, 13, 9);
        n20.AddThrone(3, 3);
        n20.AddThrone(3, 7);
        n20.AddThrone(3, 11);
        n20.AddThrone(3, 15);
        n20.AddThrone(7, 3);
        n20.AddThrone(7, 7);
        n20.AddThrone(7, 11);
        n20.AddThrone(7, 15);
        n20.AddThrone(11, 3);
        n20.AddThrone(11, 7);
        n20.AddThrone(11, 11);
        n20.AddThrone(11, 15);
        n20.AddThrone(15, 3);
        n20.AddThrone(15, 7);
        n20.AddThrone(15, 11);
        n20.AddThrone(15, 15);
        n20.AddThrone(19, 3);
        n20.AddThrone(19, 7);
        n20.AddThrone(19, 11);
        n20.AddThrone(19, 15);

        m_layouts.Add(n2);
        m_layouts.Add(n3);
        m_layouts.Add(n4);
        m_layouts.Add(n5);
        m_layouts.Add(n6);
        m_layouts.Add(n7);
        m_layouts.Add(n8);
        m_layouts.Add(n9);
        m_layouts.Add(n10);
        m_layouts.Add(n11);
        m_layouts.Add(n12);
        m_layouts.Add(n13);
        m_layouts.Add(n14);
        m_layouts.Add(n15);
        m_layouts.Add(n16);
        m_layouts.Add(n17);
        m_layouts.Add(n18);
        m_layouts.Add(n19);
        m_layouts.Add(n20);
    }
    
    static void cleanup_connections()
    {
        foreach (Node n in m_nodes)
        {
            if (n.ProvinceData.IsWater) // make sure no water provinces have incorrect connections
            {
                foreach (Connection c in n.Connections)
                {
                    c.SetConnection(ConnectionType.STANDARD);
                }
            }
            else if (n.HasNation)
            {
                foreach (Connection c in n.Connections)
                {
                    if (c.ConnectionType != ConnectionType.STANDARD)
                    {
                        c.SetConnection(ConnectionType.STANDARD);
                    }
                }
            }
        }
    }

    static void generate_roads()
    {
        int num_seas = 0;

        foreach (NationData n in m_nations)
        {
            if (n.IsWater)
            {
                num_seas += Mathf.RoundToInt(m_layout.ProvsPerPlayer * n.WaterPercentage);
            }
        }

        List<Node> starts = m_nodes.Where(x => x.HasOnlyStandardConnections && !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();
        
        if (starts.Count < 10)
        {
            starts.AddRange(m_nodes.Where(x => x.HasMostlyStandardConnections && !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater));
        }

        int total_provs = m_layout.TotalProvinces - num_seas;
        int max_roads = (int)(total_provs * (0.08f + UnityEngine.Random.Range(-0.03f, 0.03f))); // todo: expose to user?
        int count = 0;

        while (count < max_roads && starts.Any())
        {
            Node cur = starts.GetRandom();
            starts.Remove(cur);

            Connection con = cur.Connections.GetRandom();
            int limit = 0;

            while ((con.ConnectionType != ConnectionType.STANDARD || con.Node1.ProvinceData.IsWater || con.Node2.ProvinceData.IsWater) && limit < 10)
            {
                con = cur.Connections.GetRandom();
                limit++;
            }

            if (limit >= 10)
            {
                continue;
            }

            con.SetConnection(ConnectionType.ROAD);
            count++;

            if (UnityEngine.Random.Range(0, 12) == 0) // small chance to make a longer road
            {
                if (con.Node1 == cur)
                {
                    if (starts.Contains(con.Node2))
                    {
                        starts.Remove(con.Node2);
                    }

                    starts.Insert(0, con.Node2);
                }
                else
                {
                    if (starts.Contains(con.Node1))
                    {
                        starts.Remove(con.Node1);
                    }

                    starts.Insert(0, con.Node1);
                }
            }
        }
    }

    static void generate_cliffs()
    {
        List<Node> starts = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWaterSwamp).ToList();

        int max_passes = (int)(starts.Count * (0.24f + UnityEngine.Random.Range(0f, 0.02f))); // todo: expose to user?
        int max_cliffs = (int)(starts.Count * (0.20f + UnityEngine.Random.Range(-0.02f, 0.02f))); 
        int count = 0;

        while (count < max_cliffs && starts.Any())
        {
            Node cur = starts.GetRandom();
            Connection con = cur.Connections.GetRandom();
            int limit = 0;

            while ((con.ConnectionType != ConnectionType.STANDARD || con.IsTouchingSea || con.Adjacent.Any(x => x.IsTouchingSea)) && limit < 10)
            {
                con = cur.Connections.GetRandom();
                limit++;
            }

            if (limit == 10 || con == null)
            {
                continue;
            }

            if (!cur.HasMostlyStandardConnections)
            {
                starts.Remove(cur);
            }

            int len = UnityEngine.Random.Range(1, 3);
            int i = 0;

            while (i < len)
            {
                if (con.Node1.NumStandardConnections < 3 || con.Node2.NumStandardConnections < 3 || con.ConnectionType != ConnectionType.STANDARD)
                {
                    break;
                }

                con.SetConnection(ConnectionType.MOUNTAIN);

                limit = 0;
                con = con.Adjacent.GetRandom();

                while ((con.ConnectionType != ConnectionType.STANDARD || con.IsTouchingSea || con.Adjacent.Any(x => x.IsTouchingSea)) && limit < 10)
                {
                    con = cur.Connections.GetRandom();
                    limit++;
                }

                if (limit == 6)
                {
                    i = len;
                }

                i++;
                count++;
            }
        }

        count = 0;

        while (count < max_passes && starts.Any())
        {
            Node cur = starts.GetRandom();
            Connection con = cur.Connections.GetRandom();
            int limit = 0;

            while ((con.ConnectionType != ConnectionType.STANDARD || con.IsTouchingSea || con.Adjacent.Any(x => x.IsTouchingSea)) && limit < 10)
            {
                con = cur.Connections.GetRandom();
                limit++;
            }

            if (limit == 10 || con == null)
            {
                continue;
            }

            if (!cur.HasMostlyStandardConnections)
            {
                starts.Remove(cur);
            }

            int len = UnityEngine.Random.Range(1, 3);
            int i = 0;

            while (i < len)
            {
                if (con.Node1.NumStandardConnections < 3 || con.Node2.NumStandardConnections < 3 || con.ConnectionType != ConnectionType.STANDARD)
                {
                    break;
                }

                con.SetConnection(ConnectionType.MOUNTAINPASS);

                limit = 0;
                con = con.Adjacent.GetRandom();

                while ((con.ConnectionType != ConnectionType.STANDARD || con.IsTouchingSea || con.Adjacent.Any(x => x.IsTouchingSea)) && limit < 10)
                {
                    con = cur.Connections.GetRandom();
                    limit++;
                }

                if (limit == 6)
                {
                    i = len;
                }

                i++;
                count++;
            }
        }

        int num_flips = UnityEngine.Random.Range(0, max_passes - 2);

        List<Connection> mounts = m_connections.Where(x => x.ConnectionType == ConnectionType.MOUNTAIN).ToList();
        List<Connection> passes = m_connections.Where(x => x.ConnectionType == ConnectionType.MOUNTAINPASS).ToList();

        for (int i = 0; i < num_flips; i++)
        {
            Connection c1 = mounts.GetRandom();
            Connection c2 = passes.GetRandom();
            mounts.Remove(c1);
            passes.Remove(c2);

            c1.SetConnection(ConnectionType.MOUNTAINPASS);
            c2.SetConnection(ConnectionType.MOUNTAIN);
        }
    }

    static Connection get_connection_weighted(List<Connection> conns)
    {
        if (conns.Count < 5)
        {
            return conns.GetRandom();
        }

        conns.Shuffle();
        conns = conns.OrderBy(x => x.NumSeaSwamp).ToList();

        int pos = UnityEngine.Random.Range(0, Mathf.RoundToInt(conns.Count * 0.5f));

        return conns[pos];
    }

    static void generate_rivers()
    {
        List<Node> lands = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        int max_rivers = (int)(lands.Count * (0.24f + UnityEngine.Random.Range(0f, 0.02f))); // todo: expose to user?
        int max_shallow = (int)(lands.Count * (0.20f + UnityEngine.Random.Range(-0.02f, 0.02f)));
        int count = 0;

        List<Connection> tertiary = new List<Connection>();
        List<Connection> starts = new List<Connection>();
        List<Node> water = m_nodes.Where(x => x.ProvinceData.IsWaterSwamp).ToList();
        List<Connection> invalid = m_connections.Where(x => x.ConnectionType != ConnectionType.STANDARD || x.Node1.HasNation || x.Node2.HasNation || x.IsTouchingSea || (x.Node1.IsCapRing && x.Node2.IsCapRing)).ToList();

        foreach (Node n in water)
        {
            foreach (Node n1 in n.ConnectedNodes)
            {
                foreach (Node n2 in n.ConnectedNodes)
                {
                    if (n1 == n2)
                    {
                        continue;
                    }

                    Connection c = n1.GetConnectionTo(n2);

                    if (c != null && !c.IsTouchingSea && !n1.IsCapRing && !n2.IsCapRing)
                    {
                        if (!invalid.Contains(c))
                        {
                            starts.Add(c);
                        }
                    }
                }
            }
        }

        Connection cur = get_connection_weighted(starts);

        while (count < max_shallow && starts.Any())
        {
            starts.Remove(cur);

            if (!starts.Any())
            {
                starts.AddRange(tertiary);
                tertiary = new List<Connection>();
            }

            if (cur.Node1.NumStandardConnections < 3 || cur.Node2.NumStandardConnections < 3 || cur.ConnectionType != ConnectionType.STANDARD || cur.Adjacent.Any(x => x.ConnectionType == ConnectionType.MOUNTAIN || x.ConnectionType == ConnectionType.MOUNTAINPASS))
            {
                cur = get_connection_weighted(starts);
                continue;
            }

            cur.SetConnection(ConnectionType.SHALLOWRIVER);

            var adj = cur.Adjacent.Where(x => !invalid.Contains(x) && !tertiary.Contains(x) && !starts.Contains(x) && x.ConnectionType == ConnectionType.STANDARD && x.SharesNode(cur));

            if (UnityEngine.Random.Range(0, 10) < 4)
            {
                tertiary.AddRange(adj);
            }
            else
            {
                starts.AddRange(adj);
            }

            cur = get_connection_weighted(starts);
            count++;
        }

        count = 0;

        while (count < max_rivers && starts.Any())
        {
            starts.Remove(cur);

            if (!starts.Any())
            {
                starts.AddRange(tertiary);
                tertiary = new List<Connection>();
            }

            if (cur.Node1.NumStandardConnections < 3 || cur.Node2.NumStandardConnections < 3 || cur.ConnectionType != ConnectionType.STANDARD || cur.Adjacent.Any(x => x.ConnectionType == ConnectionType.MOUNTAIN || x.ConnectionType == ConnectionType.MOUNTAINPASS))
            {
                cur = get_connection_weighted(starts);
                continue;
            }

            cur.SetConnection(ConnectionType.RIVER);

            var adj = cur.Adjacent.Where(x => !invalid.Contains(x) && !tertiary.Contains(x) && !starts.Contains(x) && x.ConnectionType == ConnectionType.STANDARD && x.SharesNode(cur));

            if (UnityEngine.Random.Range(0, 10) < 4)
            {
                tertiary.AddRange(adj);
            }
            else
            {
                starts.AddRange(adj);
            }

            cur = get_connection_weighted(starts);
            count++;
        }

        int num_flips = UnityEngine.Random.Range(0, max_rivers - 2);

        List<Connection> rivers = m_connections.Where(x => x.ConnectionType == ConnectionType.RIVER).ToList();
        List<Connection> shallows = m_connections.Where(x => x.ConnectionType == ConnectionType.SHALLOWRIVER).ToList();

        for (int i = 0; i < num_flips; i++)
        {
            Connection c1 = rivers.GetRandom();
            Connection c2 = shallows.GetRandom();
            rivers.Remove(c1);
            shallows.Remove(c2);

            c1.SetConnection(ConnectionType.SHALLOWRIVER);
            c2.SetConnection(ConnectionType.RIVER);
        }
    }

    static void generate_seas()
    {
        foreach (Node n in m_starts)
        {
            if (n.Nation.IsWater)
            {
                int modifier = n.ConnectedNodes.Count + 1; // subtract their capring from the total count
                int count = 0;
                int num_water = Mathf.RoundToInt(m_layout.ProvsPerPlayer * n.Nation.WaterPercentage);

                if (modifier >= num_water) // there should be at least 1 water province added
                {
                    modifier = num_water - 1;
                }
                
                while (count < num_water - modifier)
                {
                    int iterations = UnityEngine.Random.Range(2, 7);
                    int i = 0;
                    Node cur = n.ConnectedNodes.GetRandom();

                    while (i < iterations && count < num_water - modifier)
                    {
                        if (!cur.HasNation && !cur.IsCapRing && !cur.ProvinceData.IsWater)
                        {
                            int rand = UnityEngine.Random.Range(0, 10);
                            int rand2 = UnityEngine.Random.Range(0, 10);

                            cur.ProvinceData.SetTerrainFlags(Terrain.SEA);

                            if (rand < 4) // deep sea choices
                            {
                                cur.ProvinceData.AddTerrainFlag(Terrain.DEEPSEA);

                                if (rand2 < 2)
                                {
                                    cur.ProvinceData.AddTerrainFlag(Terrain.HIGHLAND);
                                }
                                else if (rand2 < 4)
                                {
                                    cur.ProvinceData.AddTerrainFlag(Terrain.CAVE);
                                }
                            }
                            else // normal sea choices
                            {
                                if (rand2 < 2)
                                {
                                    cur.ProvinceData.AddTerrainFlag(Terrain.FARM);
                                }
                                else if (rand2 < 4)
                                {
                                    cur.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                                }
                                else if (rand2 < 5)
                                {
                                    cur.ProvinceData.AddTerrainFlag(Terrain.HIGHLAND);
                                }
                                else if (rand2 < 6)
                                {
                                    cur.ProvinceData.AddTerrainFlag(Terrain.CAVE);
                                }
                            }

                            i++;
                            count++;
                        }

                        cur = cur.ConnectedNodes.GetRandom();
                    }
                }
            }
        }
    }

    static void generate_farms(List<Node> original)
    {
        float num_farms = UnityEngine.Random.Range(0.12f, 0.15f); // 12-15% of the provinces should be farmland - todo: expose this to the user

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && x.ProvinceData.IsPlains).ToList();

        int max = Mathf.RoundToInt(num_farms * original.Count);
        int i = 0;
        
        while (i < max && valid.Count > 0)
        {
            Node n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvinceCount(Terrain.FARM) > 1)
            {
                continue;
            }

            n.ProvinceData.SetTerrainFlags(Terrain.FARM);

            i++;
        }
    }

    static void generate_thrones()
    {
        int num_water = m_starts.Where(x => x.Nation.WaterPercentage > 0.3f).Count();
        int water_ct = UnityEngine.Random.Range(-2, 1);

        foreach (Node n in m_nodes)
        {
            if (m_layout.Spawns.Any(x => x.X == n.X && n.Y == x.Y && x.SpawnType == SpawnType.THRONE))
            {
                if (n.ProvinceData.IsWater)
                {
                    water_ct++;

                    if (water_ct > num_water)
                    {
                        n.ProvinceData.SetTerrainFlags(Terrain.SWAMP);
                    }
                }

                n.ProvinceData.AddTerrainFlag(Terrain.THRONE);
            }
        }
    }

    static void generate_sized(List<Node> original)
    {
        float num_large = UnityEngine.Random.Range(0.32f, 0.38f); // 32-38% of the provinces should be large - todo: expose this to the user?
        float num_small = UnityEngine.Random.Range(0.24f, 0.30f); 

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing).ToList();
        Dictionary<NationData, List<Node>> dict = new Dictionary<NationData, List<Node>>();

        foreach (Node n in m_starts)
        {
            List<Node> nodes = new List<Node>();

            foreach (Node conn in n.ConnectedNodes)
            {
                foreach (Node t in conn.ConnectedNodes.Where(x => !x.IsCapRing && !x.HasNation))
                {
                    nodes.Add(t);
                }
            }

            dict.Add(n.Nation, nodes);
        }

        int max = Mathf.Max(Mathf.RoundToInt((num_large * original.Count) / m_starts.Count), 2);
        int i = 0;

        while (i < max) // fairly assign large provinces to each nation 
        {
            foreach (KeyValuePair<NationData, List<Node>> pair in dict)
            {
                if (pair.Value.Any())
                {
                    Node n = pair.Value.GetRandom();
                    pair.Value.Remove(n);

                    n.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);
                }
                else
                {
                    if (valid.Any())
                    {
                        Node alt = valid.GetRandom();
                        valid.Remove(alt);

                        alt.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);
                    }
                    else
                    {
                        Debug.LogError("No provinces left to tag as large");
                    }
                }
            }

            i++;
        }

        valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV)).ToList();
        max = Mathf.RoundToInt(num_small * original.Count);
        i = 0;

        while (i < max)
        {
            Node n = valid.GetRandom();
            valid.Remove(n);

            n.ProvinceData.AddTerrainFlag(Terrain.SMALLPROV);

            i++;
        }
    }

    static void generate_misc(List<Node> original)
    {
        float num_highlands = UnityEngine.Random.Range(0.06f, 0.09f); // 6-9% of the provinces should be highlands - todo: expose these to the user
        float num_mountains = UnityEngine.Random.Range(0.06f, 0.09f); 
        float num_forests = UnityEngine.Random.Range(0.12f, 0.15f);  
        float num_caves = UnityEngine.Random.Range(0.06f, 0.09f); 
        float num_waste = UnityEngine.Random.Range(0.06f, 0.09f); 

        Dictionary<Terrain, float> dict = new Dictionary<Terrain, float>();
        dict.Add(Terrain.HIGHLAND, num_highlands);
        dict.Add(Terrain.MOUNTAINS, num_mountains);
        dict.Add(Terrain.FOREST, num_forests);
        dict.Add(Terrain.CAVE, num_caves);
        dict.Add(Terrain.WASTE, num_waste);

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.IsWaterSwamp).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && x.ProvinceData.Terrain == Terrain.PLAINS).ToList();
        }

        if (valid.Count <= dict.Count) // if there's less than 1 for each type then we randomly distribute what we have
        {
            List<Terrain> terrains = new List<Terrain> { Terrain.HIGHLAND, Terrain.MOUNTAINS, Terrain.FOREST, Terrain.CAVE, Terrain.WASTE };
            terrains.Shuffle();

            foreach (Terrain t in terrains)
            {
                if (!valid.Any())
                {
                    break;
                }

                if (UnityEngine.Random.Range(0, 10) == 0) // small chance to ignore some
                {
                    continue;
                }

                Node n = valid.GetRandom();
                valid.Remove(n);

                n.ProvinceData.SetTerrainFlags(t);

                int rand = UnityEngine.Random.Range(0, 10);

                if (t == Terrain.CAVE && rand < 2)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                }
                if (t == Terrain.WASTE)
                {
                    if (rand == 0)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.WARMER);
                    }
                    else if (rand == 1)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                    }
                }
                /*if ((t == Terrain.HIGHLAND || t == Terrain.MOUNTAINS) && rand == 0)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                }*/
            }

            return;
        }

        foreach (KeyValuePair<Terrain, float> pair in dict)
        {
            int max = Mathf.Max(Mathf.RoundToInt(pair.Value * original.Count), 1);
            int i = 0;

            while (i < max && valid.Any())
            {
                Node n = valid.GetRandom();
                valid.Remove(n);

                n.ProvinceData.SetTerrainFlags(pair.Key);

                int rand = UnityEngine.Random.Range(0, 10);

                if (pair.Key == Terrain.CAVE && rand < 2)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                }
                if (pair.Key == Terrain.WASTE)
                {
                    if (rand == 0)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.WARMER);
                    }
                    else if (rand == 1)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                    }
                }
                /*if ((pair.Key == Terrain.HIGHLAND || pair.Key == Terrain.MOUNTAINS) && rand == 0)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                }*/

                i++;
            }
        }
    }

    static void generate_swamps(List<Node> original)
    {
        float num_swamps = UnityEngine.Random.Range(0.07f, 0.10f); // 7-10% of the provinces should be swamp
        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && !x.ProvinceData.IsWater).ToList();
        }

        int swamps = Mathf.Max(Mathf.RoundToInt(num_swamps * original.Count), 1);
        int i = 0;

        while (i < swamps)
        {
            if (!valid.Any())
            {
                break;
            }

            Node n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvinceCount(Terrain.SWAMP) > 1)
            {
                continue;
            }

            n.ProvinceData.SetTerrainFlags(Terrain.SWAMP);

            i++;
        }
    }

    static void generate_lakes() // random placement logic for small bodies of water
    {
        if (!m_nations.Any(x => !x.IsWater))
        {
            return;
        }

        float num_lakes = UnityEngine.Random.Range(0.06f, 0.09f); // 6-9% of the provinces should be watar - todo: expose these to the user
        
        if (m_nations.Any(x => x.WaterPercentage > 0.3f))
        {
            num_lakes = UnityEngine.Random.Range(0.02f, 0.04f); // if real water nations are playing then reduce the random water provinces slightly
        }

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && !x.ProvinceData.IsWater).ToList();
        }

        int lakes = Mathf.Max(Mathf.RoundToInt(num_lakes * valid.Count), 1);
        
        for (int i = 0; i < lakes; i++)
        {
            if (!valid.Any())
            {
                break;
            }

            Node n = valid.GetRandom();
            valid.Remove(n);

            int rand = UnityEngine.Random.Range(0, 10);
            int rand2 = UnityEngine.Random.Range(0, 10);

            n.ProvinceData.SetTerrainFlags(Terrain.SEA);

            if (rand < 4) // deep sea choices
            {
                n.ProvinceData.AddTerrainFlag(Terrain.DEEPSEA);

                if (rand2 < 2)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.HIGHLAND);
                }
                else if (rand2 < 4)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.CAVE);
                }
            }
            else // normal sea choices
            {
                if (rand2 < 2)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.FARM);
                }
                else if (rand2 < 4)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                }
                else if (rand2 < 5)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.HIGHLAND);
                }
                else if (rand2 < 6)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.CAVE);
                }
            }

            if (rand == 6) // small chance to create connected water province
            {
                Connection c = n.Connections.FirstOrDefault(x => !x.Node1.IsCapRing && !x.Node2.IsCapRing);

                if (c != null)
                {
                    if (c.Node1 == n)
                    {
                        valid.Insert(0, c.Node2);
                    }
                    else
                    {
                        valid.Insert(0, c.Node1);
                    }
                }
            }
        }  
    }

    static void generate_caprings()
    {
        foreach (Node n in m_nodes)
        {
            if (n.HasNation) // capring logic
            {
                n.Connections.Shuffle();

                int i = 0;

                foreach (Connection c in n.Connections)
                {
                    if (i >= n.Nation.TerrainData.Length)
                    {
                        break;
                    }

                    if (c.Node1 == n)
                    {
                        c.Node2.ProvinceData.SetTerrainFlags(n.Nation.TerrainData[i]);
                        c.Node2.SetAssignedTerrain(true);
                    }
                    else
                    {
                        c.Node1.ProvinceData.SetTerrainFlags(n.Nation.TerrainData[i]);
                        c.Node1.SetAssignedTerrain(true);
                    }

                    i++;
                }
            }
        }
    }

    static void generate_connections()
    {
        // connect the top right to bottom left all the time
        Node basenode = m_nodes.FirstOrDefault(n => n.X == 0 && n.Y == 0);
        Node corner = m_nodes.FirstOrDefault(n => n.X == m_layout.X - 1 && n.Y == m_layout.Y - 1);
        Connection con = new Connection(basenode, corner, ConnectionType.STANDARD, true);

        m_connections.Add(con);
        basenode.AddConnection(con);
        corner.AddConnection(con);

        // basic connections
        foreach (Node n in m_nodes)
        {
            Node up = get_node_with_wrap(n.X, n.Y + 1);
            Node right = get_node_with_wrap(n.X + 1, n.Y);
            Node down = get_node_with_wrap(n.X, n.Y - 1);
            Node left = get_node_with_wrap(n.X - 1, n.Y);

            if (!n.HasConnection(up))
            {
                Connection conn = new Connection(n, up, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                up.AddConnection(conn);
            }
            if (!n.HasConnection(right))
            {
                Connection conn = new Connection(n, right, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                right.AddConnection(conn);
            }
            if (!n.HasConnection(down))
            {
                Connection conn = new Connection(n, down, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                down.AddConnection(conn);
            }
            if (!n.HasConnection(left))
            {
                Connection conn = new Connection(n, left, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                left.AddConnection(conn);
            }
        }

        // capring connections - every player should have 5 provinces in their capring
        foreach (Node n in m_nodes.Where(x => x.HasNation))
        {
            List<Node> diag = new List<Node>();
            diag.Add(get_node_with_wrap(n.X + 1, n.Y + 1));
            diag.Add(get_node_with_wrap(n.X + 1, n.Y - 1));
            diag.Add(get_node_with_wrap(n.X - 1, n.Y - 1));
            diag.Add(get_node_with_wrap(n.X - 1, n.Y + 1));

            while (n.Connections.Count < 5) //(n.Connections.Count < n.Nation.CapRing)
            {
                Node next = diag.GetRandom();
                diag.Remove(next);

                Connection conn = new Connection(n, next, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                next.AddConnection(conn);
                n.AddConnection(conn);
            }
        }

        // random diagonal connections
        foreach (Node n in m_nodes)
        {
            if (n == corner)
            {
                continue;
            }

            Node up_right = get_node_with_wrap(n.X + 1, n.Y + 1);
            Node up = get_node_with_wrap(n.X, n.Y + 1);
            Node right = get_node_with_wrap(n.X + 1, n.Y);

            if (up_right.HasConnection(n) || up.HasConnection(right))
            {
                continue;
            }

            if (n.HasNation || up_right.HasNation)
            {
                Connection conn = new Connection(up, right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up.AddConnection(conn);
                right.AddConnection(conn);

                continue;
            }

            if (up.HasNation || right.HasNation)
            {
                Connection conn = new Connection(n, up_right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up_right.AddConnection(conn);
                n.AddConnection(conn);

                continue;
            }

            if (UnityEngine.Random.Range(0, 2) == 0 && (!n.HasNation && !up_right.HasNation))
            {
                Connection conn = new Connection(n, up_right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up_right.AddConnection(conn);
                n.AddConnection(conn);
            }
            else
            {
                Connection conn = new Connection(up, right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up.AddConnection(conn);
                right.AddConnection(conn);
            }
        }

        var caps = m_nodes.Where(x => x.HasNation);

        // designate caprings 
        foreach (Node n in caps)
        {
            foreach (Connection c in n.Connections)
            {
                if (c.Node1 != n)
                {
                    c.Node1.SetCapRing(true);
                }
                else
                {
                    c.Node2.SetCapRing(true);
                }
            }
        }

        // calculate adjacent connections
        foreach (Connection c in m_connections)
        {
            c.CalcAdjacent(m_connections, m_layout);
        }
    }

    static Node get_node_with_wrap(int x, int y)
    {
        if (x >= m_layout.X)
        {
            x = 0;
        }
        else if (x < 0)
        {
            x = m_layout.X - 1;
        }

        if (y >= m_layout.Y)
        {
            y = 0;
        }
        else if (y < 0)
        {
            y = m_layout.Y - 1;
        }

        Node n = m_nodes.FirstOrDefault(node => node.X == x && node.Y == y);
        return n;
    }

    static void generate_nodes()
    {
        m_nodes = new List<Node>();
        m_starts = new List<Node>();
        m_connections = new List<Connection>();

        List<NationData> scrambled = new List<NationData>();
        
        if (m_teamplay)
        {
            List<NationData> all = new List<NationData>();
            all.AddRange(m_nations);

            while (all.Any())
            {
                int front = UnityEngine.Random.Range(0, 2);
                int flip = UnityEngine.Random.Range(0, 2);

                NationData d1 = all[0];
                NationData d2 = all[1];
                all.Remove(d1);
                all.Remove(d2);

                if (front == 0)
                {
                    if (flip == 0)
                    {
                        scrambled.Add(d1);
                        scrambled.Add(d2);
                    }
                    else
                    {
                        scrambled.Add(d2);
                        scrambled.Add(d1);
                    }
                }
                else
                {
                    if (flip == 0)
                    {
                        scrambled.Insert(0, d1);
                        scrambled.Insert(0, d2);
                    }
                    else
                    {
                        scrambled.Insert(0, d2);
                        scrambled.Insert(0, d1);
                    }
                }
            }
        }
        else
        {
            scrambled.AddRange(m_nations);
            scrambled.Shuffle();
        }

        NodeLayout nl = m_layouts.FirstOrDefault(x => x.NumPlayers == m_num_players);

        if (nl == null)
        {
            Debug.LogError("No map layout made for " + m_num_players);
            return;
        }

        m_layout = nl;

        if (m_teamplay)
        {
            create_team_nodes(nl, scrambled);
        }
        else
        {
            List<NationData> water = new List<NationData>();

            foreach (NationData d in scrambled)
            {
                if (d.WaterPercentage > 0.3f)
                {
                    water.Add(d);
                }
            }

            foreach (NationData d in water)
            {
                scrambled.Remove(d);
            }

            create_basic_nodes(nl, scrambled, water);
        }
    }

    static void create_team_nodes(NodeLayout nl, List<NationData> nats)
    {
        for (int x = 0; x < nl.X; x++) // create all nodes first
        {
            for (int y = 0; y < nl.Y; y++)
            {
                Node node = new Node(x, y, new ProvinceData());

                if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                {
                    node.SetWrapCorner(true);
                }

                m_nodes.Add(node);
            }
        }

        int spawn_num = 0;

        while (nats.Any())
        {
            NationData d1 = nats[0];
            NationData d2 = nats[1];
            nats.Remove(d1);
            nats.Remove(d2);

            List<SpawnPoint> valid = nl.Spawns.Where(x => x.TeamNum == spawn_num).ToList();

            if (valid.Count != 2)
            {
                throw new UnityException("Invalid spawn point information for playercount: " + nl.NumPlayers);
            }

            Node n1 = m_nodes.FirstOrDefault(x => x.X == valid[0].X && x.Y == valid[0].Y);
            Node n2 = m_nodes.FirstOrDefault(x => x.X == valid[1].X && x.Y == valid[1].Y);

            if (n1 == null || n2 == null)
            {
                throw new UnityException("Invalid spawn point information for playercount: " + nl.NumPlayers);
            }

            n1.SetPlayerInfo(d1, new ProvinceData(d1.CapTerrain | Terrain.START));
            n2.SetPlayerInfo(d2, new ProvinceData(d2.CapTerrain | Terrain.START));
            n1.SetAssignedTerrain(true);
            n2.SetAssignedTerrain(true);

            m_starts.Add(n1);
            m_starts.Add(n2);
            spawn_num++;
        }
    }

    static List<Node> get_closest_nodes(Node n)
    {
        Dictionary<Node, float> dict = new Dictionary<Node, float>();
        List<Node> nodes = m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)).ToList();

        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            return nodes.OrderBy(x => n.DistanceTo(x)).ThenBy(x => Mathf.Abs(x.X - n.X)).ToList();
        }
        else
        {
            return nodes.OrderBy(x => n.DistanceTo(x)).ThenBy(x => Mathf.Abs(x.Y - n.Y)).ToList();
        }
    }

    static void create_basic_nodes(NodeLayout nl, List<NationData> nats, List<NationData> water)
    {
        for (int x = 0; x < nl.X; x++) // create all nodes first
        {
            for (int y = 0; y < nl.Y; y++)
            {
                Node node = new Node(x, y, new ProvinceData());

                if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                {
                    node.SetWrapCorner(true);
                }

                m_nodes.Add(node);

                if (nl.HasSpawn(x, y, SpawnType.PLAYER))
                {
                    node.ProvinceData.AddTerrainFlag(Terrain.START);
                    m_starts.Add(node);
                }
            }
        }

        if (m_cluster_water) // put water nations close together
        {
            List<Node> starts = m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)).ToList();

            Node start = starts.GetRandom();
            List<Node> nodes = get_closest_nodes(start);

            foreach (NationData d in water)
            {
                Node n = nodes.GetRandom();
                nodes.Remove(n);
                starts.Remove(n);

                n.SetPlayerInfo(d, new ProvinceData(d.CapTerrain | Terrain.START));
                n.SetAssignedTerrain(true);
            }

            foreach (NationData d in nats)
            {
                Node n = starts.GetRandom();
                starts.Remove(n);

                n.SetPlayerInfo(d, new ProvinceData(d.CapTerrain | Terrain.START));
                n.SetAssignedTerrain(true);
            }
        }
        else
        {
            nats.AddRange(water);

            foreach (Node n in m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)))
            {
                NationData d = nats.GetRandom();
                nats.Remove(d);

                n.SetPlayerInfo(d, new ProvinceData(d.CapTerrain | Terrain.START));
                n.SetAssignedTerrain(true);
            }
        }
    }
}
