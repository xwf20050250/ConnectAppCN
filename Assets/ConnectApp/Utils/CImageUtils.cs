using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;

namespace ConnectApp.Utils {
    public static class CImageUtils {
        const float ImageWidthMin = 200;
        const float ImageWidthMax = 4000;

        public static string SuitableSizeImageUrl(float imageWidth, string imageUrl) {
            var devicePixelRatio = Window.instance.devicePixelRatio;
            if (imageWidth <= 0) {
                Debug.Assert(imageWidth <= 0, $"Image width error, width: {imageWidth}");
            }

            var networkImageWidth = Math.Ceiling(imageWidth * devicePixelRatio);
            if (networkImageWidth <= ImageWidthMin) {
                networkImageWidth = ImageWidthMin;
            }
            else if (networkImageWidth >= ImageWidthMax) {
                networkImageWidth = ImageWidthMax;
            }

            var url = $"{imageUrl}.{networkImageWidth}x0x1.jpg";
            return url;
        }

        public static string SizeTo200ImageUrl(string imageUrl) {
            return $"{imageUrl}.200x0x1.jpg";
        }

        public static string SplashImageUrl(string imageUrl) {
            var imageWidth = Math.Ceiling(Window.instance.physicalSize.width);
            return $"{imageUrl}.{imageWidth}x0x1.jpg";
        }

        public static bool isNationalDay = false;

        public static Widget GenBadgeImage(List<string> badges, string license, EdgeInsets padding) {
            
            var badgeList  = new List<Widget>();
            Widget badgeWidget = null;
            
            if (license.isNotEmpty()) {
                if (license == "UnityPro") {
                    badgeWidget = Image.asset(
                        "image/pro-badge",
                        height: 15,
                        width: 26
                    );
                }

                if (license == "UnityPersonalPlus") {
                    badgeWidget = Image.asset(
                        "image/plus-badge",
                        height: 15,
                        width: 30
                    );
                }
            }

            if (badges != null && badges.isNotEmpty()) {
                if (badges.Any(badge => badge.isNotEmpty() && badge.Equals("official"))) {
                    badgeWidget = Image.asset(
                        "image/official-badge",
                        height: 18,
                        width: 18
                    );
                }
            }

            if (badgeWidget != null) {
                badgeList.Add(item: badgeWidget);
            }

            if (isNationalDay) {
                if (badgeList.Count >= 1) {
                    badgeList.Add(new SizedBox(width: 4));
                }
                badgeList.Add(Image.asset(
                    "image/china-flag-badge",
                    height: 14,
                    width: 16
                ));
            }
            
            if (badgeList.Count > 0) {
                return new Container(
                    padding: padding,
                    child: new Row(
                        children: badgeList
                    )
                );
            }
            
            return new Container();
        }

        public const string FavoriteCoverImagePath = "image/favorites";

        public static readonly List<string> FavoriteCoverImages = new List<string> {
            "favor-gamepad",
            "favor-programming",
            "favor-trophy",
            "favor-document",
            "favor-gamer",
            "favor-360_rotate",
            "favor-musical_note",
            "favor-book",
            "favor-smartphone",
            "favor-headphones",
            "favor-keyboard",
            "favor-game_console",
            "favor-hamburger",
            "favor-pokemon_go",
            "favor-security",
            "favor-beer",
            "favor-magazine",
            "favor-vr",
            "favor-french_fries",
            "favor-balloons",
            "favor-smartwatch",
            "favor-analytics",
            "favor-coffee_cup",
            "favor-computer",
            "favor-pencil",
            "favor-gamepad2",
            "favor-smartphone2",
            "favor-blog",
            "favor-muffin",
            "favor-camera",
            "favor-layers",
            "favor-cmyk",
            "favor-hot_air_balloon",
            "favor-video_camera",
            "favor-idea",
            "favor-map"
        };
    }
}