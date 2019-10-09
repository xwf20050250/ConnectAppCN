using System.Collections.Generic;
using ConnectApp.Constants;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Utils;
using Newtonsoft.Json;
using RSG;

namespace ConnectApp.Api {
    public static class EventApi {
        public static IPromise<FetchEventsResponse> FetchEvents(int pageNumber, string tab) {
            var promise = new Promise<FetchEventsResponse>();
            var para = new Dictionary<string, object> {
                {"tab", tab},
                {"page", pageNumber},
                {"status", tab},
                {"language", "zh_CN"}
            };
            var request = HttpManager.GET($"{Config.apiAddress}/api/connectapp/events", para);
            HttpManager.resume(request).Then(responseText => {
                var eventsResponse = JsonConvert.DeserializeObject<FetchEventsResponse>(responseText);
                promise.Resolve(eventsResponse);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }

        public static IPromise<IEvent> FetchEventDetail(string eventId) {
            var promise = new Promise<IEvent>();
            var request = HttpManager.GET($"{Config.apiAddress}/api/connectapp/events/{eventId}");
            HttpManager.resume(request).Then(responseText => {
                var liveDetail = JsonConvert.DeserializeObject<IEvent>(responseText);
                promise.Resolve(liveDetail);
            }).Catch(exception => { promise.Reject(exception); });
            return promise;
        }

        public static Promise<string> JoinEvent(string eventId) {
            var promise = new Promise<string>();
            var request = HttpManager.POST($"{Config.apiAddress}/api/connectapp/events/{eventId}/join");
            HttpManager.resume(request).Then(responseText => { promise.Resolve(eventId); })
                .Catch(exception => { promise.Reject(exception); });
            return promise;
        }
    }
}