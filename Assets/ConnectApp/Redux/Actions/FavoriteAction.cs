using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Utils;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using UnityEngine;

namespace ConnectApp.redux.actions {
    public class StartFetchFavoriteTagAction : RequestAction {
    }

    public class FetchFavoriteTagSuccessAction : BaseAction {
        public List<FavoriteTag> favoriteTags;
        public bool hasMore;
        public int offset;
    }

    public class FetchFavoriteTagFailureAction : BaseAction {
    }

    public class StartFetchFavoriteDetailAction : RequestAction {
    }

    public class FetchFavoriteDetailSuccessAction : BaseAction {
        public Dictionary<string, FavoriteTag> tagMap;
        public Dictionary<string, Article> projectSimpleMap;
        public List<Favorite> favorites;
        public bool hasMore;
        public string tagId;
        public string userId;
        public int offset;
    }

    public class FetchFavoriteDetailFailureAction : BaseAction {
    }

    public class CreateFavoriteTagSuccessAction : BaseAction {
        public FavoriteTag favoriteTag;
    }

    public class EditFavoriteTagSuccessAction : BaseAction {
        public FavoriteTag favoriteTag;
    }

    public class DeleteFavoriteTagSuccessAction : BaseAction {
        public FavoriteTag favoriteTag;
    }

    public static partial class Actions {
        public static object fetchFavoriteTags(string userId, int offset) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                var favoriteTagIdCount = getState().favoriteState.favoriteTagIds.Count;
                if (offset != 0 && offset != favoriteTagIdCount) {
                    offset = favoriteTagIdCount;
                }

                return FavoriteApi.FetchFavoriteTags(userId: userId, offset: offset)
                    .Then(favoritesResponse => {
                        dispatcher.dispatch(new FetchFavoriteTagSuccessAction {
                            offset = offset,
                            hasMore = favoritesResponse.hasMore,
                            favoriteTags = favoritesResponse.favoriteTags
                        });
                    })
                    .Catch(error => {
                        dispatcher.dispatch(new FetchFavoriteTagFailureAction());
                        Debug.Log(error);
                    });
            });
        }

        public static object fetchFavoriteDetail(string userId, string tagId, int offset) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                var favoriteTagId = tagId.isNotEmpty() ? tagId : $"{userId}all";
                var favoriteDetailArticleIds = getState().favoriteState.favoriteDetailArticleIdDict.ContainsKey(key: favoriteTagId)
                    ? getState().favoriteState.favoriteDetailArticleIdDict[key: favoriteTagId]
                    : new List<string>();
                var favoriteDetailArticleCount = favoriteDetailArticleIds.Count;
                if (offset != 0 && offset != favoriteDetailArticleCount) {
                    offset = favoriteDetailArticleCount;
                }

                return FavoriteApi.FetchFavoriteDetail(userId: userId, tagId: tagId, offset: offset)
                    .Then(favoriteDetailResponse => {
                        dispatcher.dispatch(new UserMapAction {userMap = favoriteDetailResponse.userMap});
                        dispatcher.dispatch(new TeamMapAction {teamMap = favoriteDetailResponse.teamMap});
                        dispatcher.dispatch(new FetchFavoriteDetailSuccessAction {
                            tagMap = favoriteDetailResponse.tagMap,
                            projectSimpleMap = favoriteDetailResponse.projectSimpleMap,
                            favorites = favoriteDetailResponse.favorites,
                            hasMore = favoriteDetailResponse.hasMore,
                            tagId = tagId,
                            userId = userId,
                            offset = offset
                        });
                    })
                    .Catch(error => {
                        dispatcher.dispatch(new FetchFavoriteDetailFailureAction());
                        Debug.Log(error);
                    });
            });
        }

        public static object createFavoriteTag(IconStyle iconStyle, string name, string description = "") {
            if (HttpManager.isNetWorkError()) {
                CustomDialogUtils.showToast("请检查网络", iconData: Icons.sentiment_dissatisfied);
                return null;
            }

            CustomDialogUtils.showCustomDialog(
                child: new CustomLoadingDialog(
                    message: "新建收藏夹中"
                )
            );
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return FavoriteApi.CreateFavoriteTag(iconStyle: iconStyle, name: name, description: description)
                    .Then(createFavoriteTagResponse => {
                        CustomDialogUtils.hiddenCustomDialog();
                        dispatcher.dispatch(new CreateFavoriteTagSuccessAction {
                            favoriteTag = createFavoriteTagResponse
                        });
                        dispatcher.dispatch(new MainNavigatorPopAction());
                    })
                    .Catch(error => {
                        CustomDialogUtils.hiddenCustomDialog();
                        Debug.Log(error);
                    });
            });
        }

        public static object editFavoriteTag(IconStyle iconStyle, string tagId, string name, string description = "") {
            if (HttpManager.isNetWorkError()) {
                CustomDialogUtils.showToast("请检查网络", iconData: Icons.sentiment_dissatisfied);
                return null;
            }

            CustomDialogUtils.showCustomDialog(
                child: new CustomLoadingDialog(
                    message: "编辑收藏夹中"
                )
            );
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return FavoriteApi.EditFavoriteTag(tagId: tagId, iconStyle: iconStyle, name: name, description: description)
                    .Then(editFavoriteTagResponse => {
                        CustomDialogUtils.hiddenCustomDialog();
                        dispatcher.dispatch(new EditFavoriteTagSuccessAction {
                            favoriteTag = editFavoriteTagResponse
                        });
                        dispatcher.dispatch(new MainNavigatorPopAction());
                    })
                    .Catch(error => {
                        CustomDialogUtils.hiddenCustomDialog();
                        Debug.Log(error);
                    });
            });
        }

        public static object deleteFavoriteTag(string tagId) {
            if (HttpManager.isNetWorkError()) {
                CustomDialogUtils.showToast("请检查网络", iconData: Icons.sentiment_dissatisfied);
                return null;
            }

            CustomDialogUtils.showCustomDialog(
                child: new CustomLoadingDialog(
                    message: "删除收藏夹中"
                )
            );
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return FavoriteApi.DeleteFavoriteTag(tagId: tagId)
                    .Then(deleteFavoriteTagResponse => {
                        CustomDialogUtils.hiddenCustomDialog();
                        dispatcher.dispatch(new DeleteFavoriteTagSuccessAction {
                            favoriteTag = deleteFavoriteTagResponse
                        });
                    })
                    .Catch(error => {
                        CustomDialogUtils.hiddenCustomDialog();
                        Debug.Log(error);
                    });
            });
        }
    }
}