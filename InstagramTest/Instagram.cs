using System.Collections.Generic;

namespace InstagramTest
{
    public class Graphql
    {
        public User user;
    }

    public class Data
    {
        public User user;            
    }

    public class User
    {
        public string id;
        public string full_name;
        public Edge_owner_to_timeline_media edge_owner_to_timeline_media;
        public bool is_private;
    }

    public class Edge_owner_to_timeline_media
    {
        public int count;
        public List<Edge> edges;
        public Page_info page_info;
    }

    public class Page_info
    {
        public bool has_next_page;
        public string end_cursor;
    }

    public class Edge
    {
        public Node node;
    }

    public class Node
    {
        public string __typename;
        public string display_url;
    }

    public class Instagram
    {
        public Graphql graphql;
        public Data data;
    }
}