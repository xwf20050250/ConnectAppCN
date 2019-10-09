using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class FavoriteState {
        public bool favoriteTagLoading { get; set; }
        public bool favoriteDetailLoading { get; set; }
        public List<string> favoriteTagIds { get; set; }
        public Dictionary<string, List<string>> favoriteDetailArticleIdDict { get; set; }
        public bool favoriteTagHasMore { get; set; }
        public bool favoriteDetailHasMore { get; set; }
        public Dictionary<string, FavoriteTag> favoriteTagDict { get; set; }
    }
}