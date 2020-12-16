using System;
using System.Net;
using System.Drawing;
using System.Net.Http;
using System.Threading;
using System.Reflection;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HabboGallery.Desktop.Habbo;
using HabboGallery.Desktop.Web.Json;

using Sulakore.Habbo;
using Sulakore.Network;

namespace HabboGallery.Desktop.Web
{
    //TODO: Re-write for the proper API endpoints
    public class ApiClient : HttpClient
    {
        private const string CSRF_PATTERN = "<meta\\s+name=\"csrf-token\"\\s+content=\"(?<token>[a-zA-Z0-9]{40})\">";

        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;

        private string _loginKey;

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
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("User-Agent", "HabboGallery.Desktop Agent v" + Assembly.GetExecutingAssembly().GetName().Version);
        }

        public async Task<ApiResponse<GalleryRecord>> StorePhotoAsync(PhotoItem photoItem)
        {
            Dictionary<string, string> parameters = new()
            {
                { "item_id", photoItem.Id.ToString() },
                { "game_checksum", photoItem.Checksum.ToString() },
                { "owner_name", photoItem.OwnerName },
                { "room_id", photoItem.RoomId?.ToString() ?? null },
                { "country_code", HotelEndPoint.GetRegion(photoItem.Hotel) },
                { "description", photoItem.Description },
                { "date", new DateTimeOffset(photoItem.Date).ToUnixTimeSeconds().ToString() },
                { "login_key", _loginKey }
            };

            using var response = await _client.PostAsync("api/photos/store",
                new FormUrlEncodedContent(parameters)).ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync<ApiResponse<GalleryRecord>>().ConfigureAwait(false);
        }

        public async Task<IEnumerable<int>> BatchCheckExistingIdsAsync(IEnumerable<int> ids, HHotel hotel)
        {
            var request = new BatchRequest(_loginKey, HotelEndPoint.GetRegion(hotel), ids);
            using var response = await _client.PostAsJsonAsync("api/photos/checkexisting", request).ConfigureAwait(false);
            
            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<int>>().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return ids;
            }
        }

        public Task<ApiResponse<GalleryRecord>> GetPhotoByIdAsync(int photoId, HHotel hotel)
        {
            return _client.GetFromJsonAsync<ApiResponse<GalleryRecord>>(
                $"api/photos/byid/{HotelEndPoint.GetRegion(hotel)}/{photoId}");
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            string token = await FetchTokenAsync().ConfigureAwait(false);

            Dictionary<string, string> parameters = new()
            {
                { "email", email },
                { "password", password },
                { "_token", token },
                { "external", "true" }
            };
            
            using var response = await _client.PostAsync("login", 
                new FormUrlEncodedContent(parameters)).ConfigureAwait(false);
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    _loginKey = _cookieContainer.GetCookies(_client.BaseAddress)["login_key"].Value;
                    IsAuthenticated = true;
                }
                catch (Exception) { } //TODO: Actually handle this, the cookie isn't set if this exception is thrown
            }

            return IsAuthenticated;
        }

        public async Task<double> GetLatestVersionAsync()
        {
            string content = await _client.GetStringAsync("api/desktop/version").ConfigureAwait(false);

            double version = Constants.APP_VERSION;
            double.TryParse(content, out version);

            return version;
        }

        public async Task<string> FetchTokenAsync()
        {
            string content = await _client.GetStringAsync("login").ConfigureAwait(false);
            Match tokenMatch = Regex.Match(content, CSRF_PATTERN);

            return tokenMatch.Success ? 
                tokenMatch.Groups["token"].Value : throw new Exception("No CSRF token found");
        }

        public async Task<Image> GetImageAsync(string url, CancellationToken cancellationToken = default)
        {
            return Image.FromStream(await _client.GetStreamAsync(url, cancellationToken).ConfigureAwait(false));
        }
    }
}