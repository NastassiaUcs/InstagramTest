using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

namespace InstagramTest
{
    internal class Web
    {
        private string baseUrl = "https://www.instagram.com";
        private string profileInfoUrl = "{0}/{1}/?__a=1";
        private string htmlUrl = "{0}/{1}/";

        private const string REGEX_URL = @"(?<url>\/static\/bundles\/base\/ProfilePageContainer\.js\/.+?\.js)";
        private const string REGEX_QUERY = "e\\.profilePosts\\.byUserId\\.get\\(t\\)\\)\\?o\\.pagination:o\\}\\,queryId:\"(?<queryHash>.+?)\"";

        private string baseUrlGraphql = "{0}/graphql/query/?query_hash={1}";

        // количество постов для запроса, беру с запасом на случай если в ленте много видео
        public static int count = 50;

        internal List<Edge> GetLinksPhotos(string username)
        {
            string json = DownloadData(String.Format(profileInfoUrl, baseUrl, username));
            Instagram instagram = null;
            if (!String.IsNullOrEmpty(json))
            {
                try
                {
                    instagram = JsonConvert.DeserializeObject<Instagram>(json);
                }
                catch (Exception e)
                {
                    instagram = null;
                    Logger.Error("Error: {0}", e.Message);
                }

                if (instagram != null)
                {
                    if (!instagram.graphql.user.is_private)
                    {
                        if (instagram.graphql.user.edge_owner_to_timeline_media.page_info.has_next_page)
                        {
                            string html = DownloadData(String.Format(htmlUrl, baseUrl, username));
                            string queryHash = GetQueryHash(html);                            
                            string uriGraphql = GetUriGraphql(queryHash, instagram.graphql.user.id, instagram.graphql.user.edge_owner_to_timeline_media.page_info.end_cursor);
                            string dataGraphql = DownloadData(uriGraphql);
                            Instagram instTemp = JsonConvert.DeserializeObject<Instagram>(dataGraphql);
                            if (instTemp != null)
                            {
                                foreach (Edge e in instTemp.data.user.edge_owner_to_timeline_media.edges)
                                {
                                    instagram.graphql.user.edge_owner_to_timeline_media.edges.Add(e);
                                }
                            }
                            return instagram.graphql.user.edge_owner_to_timeline_media.edges;
                        }
                        else
                        {
                            Logger.Warn("У пользователя всего фото на одну страницу");
                            return instagram.graphql.user.edge_owner_to_timeline_media.edges;
                        }
                        
                    }
                    else
                    {
                        Logger.Warn("Профиль пользователя приватный, невозможно получить ссылки.");
                    }
                }                                
            }
            return null;
        }

        private string GetUriGraphql(string queryHash, string id, string end_cursor)
        {
            string temp = String.Format(baseUrlGraphql, baseUrl, queryHash);
            string variables = GetVariables(id, end_cursor);
            temp += "&variables={" + variables + "}";
            return temp;
         }

        private string GetVariables(string id, string end_cursor)
        {
            string variables = String.Format("\"id\":\"{0}\",\"first\":{1},\"after\":\"{2}\"", id, count, end_cursor);
            variables = HttpUtility.UrlEncode(variables);
            return variables;
        }

        private string GetQueryHash(string html)
        {
            Regex regex = new Regex(REGEX_URL);
            if (regex.IsMatch(html))
            {
                string jsUrl = baseUrl + regex.Match(html).Groups["url"].Value;
                string data = DownloadData(jsUrl);
                regex = new Regex(REGEX_QUERY);
                if (regex.IsMatch(data))
                {
                    string queryHash = regex.Match(data).Groups["queryHash"].Value;
                    return queryHash;
                }
            }

            return null;
        }

        private string DownloadData(string url)
        {
            string data = default;
            try
            {
                using (HttpClient client = new HttpClient())
                {  
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            data = content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                data = default;
                Logger.Error("Error: {0}", e.Message);
            }
            return data;
        }
    }
}