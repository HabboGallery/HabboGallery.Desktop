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

#nullable enable
namespace HabboGallery.Desktop.Web
{
    //TODO: Re-write for the proper API endpoints
    public class ApiClient : HttpClient
    {
        private static readonly Regex CsrfRegex = new("<meta\\s+name=\"csrf-token\"\\s+content=\"(?<token>[a-zA-Z0-9]{40})\">", RegexOptions.Compiled);

        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;

        private string? _loginKey;

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

        public async Task<ApiResponse<GalleryRecord>?> StorePhotoAsync(PhotoItem photoItem)
        {
            var parameters = new KeyValuePair<string?, string?>[]
            {
                new("item_id", photoItem.Id.ToString()),
                new("game_checksum", photoItem.Checksum.ToString()),
                new("owner_name", photoItem.OwnerName),
                new("room_id", photoItem.RoomId?.ToString() ?? null),
                new("country_code", HotelEndPoint.GetRegion(photoItem.Hotel)),
                new("description", photoItem.Description),
                new("date", new DateTimeOffset(photoItem.Date).ToUnixTimeSeconds().ToString()),
                new("login_key", _loginKey)
            };

            using var response = await _client.PostAsync("api/photos/store",
                new FormUrlEncodedContent(parameters)).ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync<ApiResponse<GalleryRecord>>().ConfigureAwait(false);
        }

        public async Task<IEnumerable<long>?> BatchCheckExistingIdsAsync(IEnumerable<long> ids, HHotel hotel)
        {
            var request = new BatchRequest(_loginKey, HotelEndPoint.GetRegion(hotel), ids);
            using var response = await _client.PostAsJsonAsync("api/photos/checkexisting", request).ConfigureAwait(false);
            
            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<long>>().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return ids;
            }
        }

        public async Task<ApiResponse<GalleryRecord>?> GetPhotoByIdAsync(long photoId, HHotel hotel)
        {
            using var response = await _client.GetAsync($"api/photos/byid/{HotelEndPoint.GetRegion(hotel)}/{photoId}").ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<GalleryRecord>>().ConfigureAwait(false);
            else return default;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            string token = await FetchTokenAsync().ConfigureAwait(false);

            var parameters = new KeyValuePair<string?, string?>[] {
                new("email", email),
                new("password", password),
                new("_token", token),
                new("external", "true")
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

        public async Task<Version?> GetLatestVersionAsync()
        {
            string content = await _client.GetStringAsync("api/desktop/version").ConfigureAwait(false);

            Version.TryParse(content, out Version? version);
            return version;
        }

        public async Task<string> FetchTokenAsync()
        {
            string content = await _client.GetStringAsync("login").ConfigureAwait(false);
            Match tokenMatch = CsrfRegex.Match(content);

            return tokenMatch.Success ? 
                tokenMatch.Groups["token"].Value : throw new Exception("No CSRF token found");
        }

        public async Task<Image> TryGetImageAsync(string url, CancellationToken cancellationToken = default)
        {
            return Image.FromStream(await _client.GetStreamAsync(url, cancellationToken).ConfigureAwait(false));
        }
    }
}