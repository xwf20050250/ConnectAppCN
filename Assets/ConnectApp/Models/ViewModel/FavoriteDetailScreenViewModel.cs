using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.ViewModel {
    public class FavoriteDetailScreenViewModel {
        public string tagId;
        public bool favoriteDetailLoading;
        public List<string> favoriteDetailArticleIds;
        public int favoriteArticleOffset;
        public bool favoriteArticleHasMore;
        public bool isLoggedIn;
        public FavoriteTag favoriteTag;
        public Dictionary<string, Article> articleDict;
        public Dictionary<string, User> userDict;
        public Dictionary<string, Team> teamDict;
    }
}