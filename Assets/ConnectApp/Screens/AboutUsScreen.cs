using System.Collections.Generic;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class AboutUsScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, object>(
                converter: state => null,
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new AboutUsScreenActionModel {
                        mainRouterPop = () => dispatcher.dispatch(new MainNavigatorPopAction()),
                        toOriginCode = () => dispatcher.dispatch(new OpenUrlAction {url = Config.originCodeUrl}),
                        toWidgetOriginCode = () =>
                            dispatcher.dispatch(new OpenUrlAction {url = Config.widgetOriginCodeUrl})
                    };
                    return new AboutUsScreen(actionModel);
                }
            );
        }
    }

    public class AboutUsScreen : StatelessWidget {
        public AboutUsScreen(
            AboutUsScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.actionModel = actionModel;
        }

        readonly AboutUsScreenActionModel actionModel;

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Container(
                        color: new Color(0xFFFAFAFA),
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(),
                                this._buildContent()
                            }
                        )
                    )
                )
            );
        }

        Widget _buildNavigationBar() {
            return new CustomAppBar(
                () => this.actionModel.mainRouterPop(),
                new Text(
                    "关于我们",
                    style: CTextStyle.PXLargeMedium
                )
            );
        }

        Widget _buildContent() {
            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(top: 44),
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            margin: EdgeInsets.only(bottom: 16),
                            alignment: Alignment.center,
                            child: new Icon(
                                Icons.UnityLogo,
                                size: 64
                            )
                        ),
                        new Text(
                            "unity Connect",
                            style: CTextStyle.H4
                        ),
                        new Container(height: 8),
                        new Text(
                            $"版本号：{Config.versionNumber} ({Config.versionCode})",
                            style: CTextStyle.PRegularBody4
                        ),
                        new Container(
                            padding: EdgeInsets.only(32, 24, 32, 32),
                            child: new Text("Unity Connect 是使用 UIWidgets 开发的移动端项目，是一个开放而友好的社区，每个开发者都能在这里学习或者分享自己的作品。",
                                style: CTextStyle.PRegularBody)),
                        new Container(color: new Color(0xFFFAFAFA), height: 16),
                        new GestureDetector(
                            child: this._tapRow("关注本项目源代码"),
                            onTap: this.pushOriginCode
                        ),
                        new GestureDetector(
                            child: this._tapRow("关注 UIWidgets 项目源代码"),
                            onTap: this.pushWidgetOriginCode
                        )
                    }
                )
            );
        }

        void pushOriginCode() {
            this.actionModel.toOriginCode();
        }

        void pushWidgetOriginCode() {
            this.actionModel.toWidgetOriginCode();
        }

        Widget _tapRow(string content) {
            return new Container(
                height: 60,
                color: CColors.Transparent,
                padding: EdgeInsets.only(16, right: 16),
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new Text(content, style: CTextStyle.PLargeBody),
                        new Icon(
                            icon: Icons.arrow_forward,
                            size: 16,
                            color: CColors.Icon
                        )
                    }));
        }
    }
}