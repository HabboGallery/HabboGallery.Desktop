using System;
using System.IO;
using System.Net;
using System.Drawing;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HabboGallery.Habbo;

using Sulakore.Network;
using Newtonsoft.Json;
using HabboGallery.Web.Json;
using HabboGallery.Properties;
using Sulakore.Habbo;
using System.Text;

namespace HabboGallery.Web
{
    public class ApiClient : HttpClient
    {
        private const string CsrfExpression = "<meta\\s+name=\"csrf-token\"\\s+content=\"(?<token>[a-zA-Z0-9]{40})\">";

        private readonly HttpClient _client;

        private string _loginKey;
        private readonly CookieContainer _cookieContainer;

        public bool IsAuthenticated { get; set; }

        public ApiClient(Uri baseAddress)
        {
            HttpMessageHandler handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookieContainer = new CookieContainer()
            };
            _client = new HttpClient(handler)
            {
                BaseAddress = baseAddress
            };
            _client.DefaultRequestHeaders.Add("User-Agent", "HabboGallery.Desktop Agent v" + Assembly.GetExecutingAssembly().GetName().Version);
        }

        public async Task<ApiResponse<OldPhoto>> PublishPhotoDataAsync(OldPhoto photo, string ownerName, int roomId)
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "photos/store"))
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {"item_id", photo.Id.ToString() },
                    {"game_checksum", photo.Checksum.ToString() },
                    {"owner_name", ownerName },
                    {"room_id", roomId == 0 ? null : roomId.ToString() },
                    {"country_code", HotelEndPoint.GetRegion(photo.Hotel) },
                    {"description", photo.Description },
                    {"date", photo.UnixTime.ToString() },
                    {"login_key", _loginKey }
                };

                message.Content = new FormUrlEncodedContent(parameters);

                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    string jsonBody = await response.Content.ReadAsStringAsync();
                    ApiResponse<OldPhoto> responseData = JsonConvert.DeserializeObject<ApiResponse<OldPhoto>>(jsonBody);
                    //TODO: try catch this whole using and return a custom responseData if server errors and does not return valid json
                    responseData.Success = response.IsSuccessStatusCode;

                    return responseData;
                }
            }
        }

        public async Task<int[]> BatchCheckExistingIdsAsync(int[] ids, HHotel hotel)
        {
            BatchRequest request = new BatchRequest(_loginKey, HotelEndPoint.GetRegion(hotel), ids);

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "photos/checkexisting"))
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    try
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<int[]>(content);
                    }
                    catch (Exception)
                    {
                        return ids;
                    }
                }
            }
        }

        public async Task<ApiResponse<OldPhoto>> GetPhotoByIdAsync(int photoId, HHotel hotel)
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, $"photos/byid/{HotelEndPoint.GetRegion(hotel)}/{photoId}"))
            {
                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    string jsonBody = await response.Content.ReadAsStringAsync();
                    ApiResponse<OldPhoto> responseData = JsonConvert.DeserializeObject<ApiResponse<OldPhoto>>(jsonBody);
                    responseData.Success = response.IsSuccessStatusCode;

                    return responseData;
                }
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "login"))
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {"email", email },
                    {"password", password },
                    {"_token", await FetchTokenAsync() },
                    {"external", "true" }
                };
                message.Content = new FormUrlEncodedContent(parameters);

                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            _loginKey = _cookieContainer.GetCookies(_client.BaseAddress)["login_key"].Value;
                            IsAuthenticated = true;
                        }
                        catch (Exception) { } //TODO: Actually handle this, the cookie isn't set if this exception is thrown
                    }
                }
            }

            return IsAuthenticated;
        }

        public async Task<double> GetLatestVersionAsync()
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "desktop/version"))
            {
                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    string content = await response.Content.ReadAsStringAsync();
                    double version = Constants.APP_VERSION;

                    double.TryParse(content, out version);
                    return version;
                }
            }
        }

        public async Task<string> FetchTokenAsync()
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "login"))
            {
                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    string content = await response.Content.ReadAsStringAsync();
                    Match tokenMatch = Regex.Match(content, CsrfExpression);

                    return tokenMatch.Success ? tokenMatch.Groups["token"].Value : throw new Exception("No CSRF token found");
                }
            }
        }

        private static Image GetImageFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms);
            }
        }

        public async Task<Image> GetPhotoAsync(Uri url)
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url.AbsolutePath))
            {
                using (HttpResponseMessage response = await _client.SendAsync(message))
                {
                    byte[] content = await response.Content.ReadAsByteArrayAsync();
                    return GetImageFromBytes(content);
                }
            }
        }
    }
}