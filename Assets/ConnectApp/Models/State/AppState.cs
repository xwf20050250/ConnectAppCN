using System;
using System.Collections.Generic;
using ConnectApp.Constants;
using ConnectApp.Models.Model;
using ConnectApp.Utils;

namespace ConnectApp.Models.State {
    [Serializable]
    public class AppState {
        public LoginState loginState { get; set; }
        public ServiceConfigState serviceConfigState { get; set; }
        public ArticleState articleState { get; set; }
        public EventState eventState { get; set; }
        public PopularSearchState popularSearchState { get; set; }
        public SearchState searchState { get; set; }
        public NotificationState notificationState { get; set; }
        public UserState userState { get; set; }
        public TeamState teamState { get; set; }
        public PlaceState placeState { get; set; }
        public FollowState followState { get; set; }
        public LikeState likeState { get; set; }
        public MineState mineState { get; set; }
        public MessageState messageState { get; set; }
        public SettingState settingState { get; set; }
        public ReportState reportState { get; set; }
        public FeedbackState feedbackState { get; set; }
        public TabBarState tabBarState { get; set; }
        public FavoriteState favoriteState { get; set; }

        public static AppState initialState() {
            var loginInfo = UserInfoManager.initUserInfo();
            var isLogin = UserInfoManager.isLogin();

            return new AppState {
                loginState = new LoginState {
                    email = "",
                    password = "",
                    loginInfo = loginInfo,
                    isLoggedIn = isLogin,
                    loading = false
                },
                serviceConfigState = new ServiceConfigState {
                    showFirstEgg = false,
                    scanEnabled = false,
                    nationalDayEnabled = false,
                },
                articleState = new ArticleState {
                    recommendArticleIds = new List<string>(),
                    followArticleIdDict = new Dictionary<string, List<string>>(),
                    hotArticleIdDict = new Dictionary<string, List<string>>(),
                    articleDict = new Dictionary<string, Article>(),
                    articlesLoading = false,
                    followArticlesLoading = false,
                    articleDetailLoading = false,
                    hottestHasMore = true,
                    feedHasNew = false,
                    feedIsFirst = false,
                    followArticleHasMore = false,
                    hotArticleHasMore = false,
                    hotArticlePage = 0,
                    beforeTime = "",
                    afterTime = "",
                    articleHistory = HistoryManager.articleHistoryList(isLogin ? loginInfo.userId : null),
                    blockArticleList = HistoryManager.blockArticleList(isLogin ? loginInfo.userId : null)
                },
                eventState = new EventState {
                    ongoingEvents = new List<string>(),
                    eventsDict = new Dictionary<string, IEvent>(),
                    ongoingEventTotal = 0,
                    completedEvents = new List<string>(),
                    completedEventTotal = 0,
                    pageNumber = 1,
                    completedPageNumber = 1,
                    eventsOngoingLoading = false,
                    eventsCompletedLoading = false,
                    eventHistory = HistoryManager.eventHistoryList(isLogin ? loginInfo.userId : null),
                    channelId = ""
                },
                popularSearchState = new PopularSearchState {
                    popularSearchArticles = new List<PopularSearch>(),
                    popularSearchUsers = new List<PopularSearch>()
                },
                searchState = new SearchState {
                    searchArticleLoading = false,
                    searchUserLoading = false,
                    searchTeamLoading = false,
                    searchFollowingLoading = false,
                    keyword = "",
                    searchFollowingKeyword = "",
                    searchArticleIdDict = new Dictionary<string, List<string>>(),
                    searchUserIdDict = new Dictionary<string, List<string>>(),
                    searchTeamIdDict = new Dictionary<string, List<string>>(),
                    searchFollowings = new List<User>(),
                    searchArticleCurrentPage = 0,
                    searchArticlePages = new List<int>(),
                    searchUserHasMore = false,
                    searchTeamHasMore = false,
                    searchFollowingHasMore = false,
                    searchArticleHistoryList =
                        HistoryManager.searchArticleHistoryList(isLogin ? loginInfo.userId : null)
                },
                notificationState = new NotificationState {
                    loading = false,
                    page = 1,
                    pageTotal = 1,
                    notifications = new List<Notification>(),
                    mentions = new List<User>()
                },
                userState = new UserState {
                    userLoading = false,
                    userArticleLoading = false,
                    followingLoading = false,
                    followingUserLoading = false,
                    followingTeamLoading = false,
                    followerLoading = false,
                    userDict = UserInfoManager.initUserDict(),
                    slugDict = new Dictionary<string, string>(),
                    userLicenseDict = new Dictionary<string, UserLicense>(),
                    fullName = "",
                    title = "",
                    jobRole = new JobRole(),
                    place = ""
                },
                teamState = new TeamState {
                    teamLoading = false,
                    teamArticleLoading = false,
                    followerLoading = false,
                    memberLoading = false,
                    teamDict = new Dictionary<string, Team>(),
                    slugDict = new Dictionary<string, string>()
                },
                placeState = new PlaceState {
                    placeDict = new Dictionary<string, Place>()
                },
                followState = new FollowState {
                    followDict = new Dictionary<string, Dictionary<string, bool>>()
                },
                likeState = new LikeState {
                    likeDict = new Dictionary<string, Dictionary<string, bool>>()
                },
                mineState = new MineState {
                    futureEventsList = new List<IEvent>(),
                    pastEventsList = new List<IEvent>(),
                    futureListLoading = false,
                    pastListLoading = false,
                    futureEventTotal = 0,
                    pastEventTotal = 0
                },
                messageState = new MessageState {
                    channelMessageDict = new Dictionary<string, Dictionary<string, Message>>(),
                    channelMessageList = new Dictionary<string, List<string>>()
                },
                settingState = new SettingState {
                    hasReviewUrl = false,
                    reviewUrl = ""
                },
                reportState = new ReportState {
                    loading = false
                },
                feedbackState = new FeedbackState {
                    feedbackType = FeedbackType.Advice,
                    loading = false
                },
                tabBarState = new TabBarState {
                    currentTabIndex = 0
                },
                favoriteState = new FavoriteState {
                    favoriteTagLoading = false,
                    favoriteDetailLoading = false,
                    favoriteTagIds = new List<string>(),
                    favoriteDetailArticleIdDict = new Dictionary<string, List<string>>(),
                    favoriteTagHasMore = false,
                    favoriteDetailHasMore = false,
                    favoriteTagDict = new Dictionary<string, FavoriteTag>()
                }
            };
        }
    }
}