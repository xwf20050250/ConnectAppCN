using System;
using System.Collections.Generic;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using ConnectApp.Utils;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    static class LoginNavigatorRoutes {
        public const string Root = "/";
        public const string BindUnity = "/bind-unity";
        public const string WechatBindUnity = "/wechat-bind-unity";
    }

    public class LoginScreen : StatelessWidget {
        static readonly GlobalKey globalKey = GlobalKey.key("login-router");

        public static NavigatorState navigator {
            get { return globalKey.currentState as NavigatorState; }
        }

        static Dictionary<string, WidgetBuilder> loginRoutes {
            get {
                return new Dictionary<string, WidgetBuilder> {
                    {LoginNavigatorRoutes.Root, context => new LoginSwitchScreenConnector()},
                    {LoginNavigatorRoutes.BindUnity, context => new BindUnityScreenConnector(FromPage.login)},
                    {LoginNavigatorRoutes.WechatBindUnity, context => new BindUnityScreenConnector(FromPage.wechat)}
                };
            }
        }

        public override Widget build(BuildContext context) {
            return new Navigator(
                key: globalKey,
                onGenerateRoute: settings => {
                    return new PageRouteBuilder(
                        settings: settings,
                        (context1, animation, secondaryAnimation) => loginRoutes[settings.name](context1),
                        (context1, animation, secondaryAnimation, child) => new PushPageTransition(
                            routeAnimation: animation,
                            child: child
                        )
                    );
                }
            );
        }
    }

    public class LoginSwitchScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, object>(
                converter: state => null,
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new LoginSwitchScreenActionModel {
                        mainRouterPop = () => dispatcher.dispatch(new MainNavigatorPopAction()),
                        loginByWechatAction = code => dispatcher.dispatch<IPromise>(Actions.loginByWechat(code)),
                        loginRouterPushToBindUnity =
                            () => dispatcher.dispatch(new LoginNavigatorPushToBindUnityAction()),
                        openUrl = url => dispatcher.dispatch(new OpenUrlAction {url = url})
                    };
                    return new LoginSwitchScreen(actionModel);
                }
            );
        }
    }

    public class LoginSwitchScreen : StatefulWidget {
        public LoginSwitchScreen(
            LoginSwitchScreenActionModel actionModel,
            Key key = null
        ) : base(key: key) {
            this.actionModel = actionModel;
        }

        public readonly LoginSwitchScreenActionModel actionModel;

        public override State createState() {
            return new _LoginSwitchScreen();
        }
    }

    class _LoginSwitchScreen : State<LoginSwitchScreen>, RouteAware {
        
        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Router.routeObserve.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Router.routeObserve.unsubscribe(this);
            base.dispose();
        }

        public void didPop() {
            QRScanPlugin.qrCodeToken = null;
        }

        public void didPopNext() {
        }

        public void didPush() {
        }

        public void didPushNext() {
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: this._buildContent(context: context)
                )
            );
        }

        Widget _buildContent(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        this._buildTopView(),
                        this._buildBottomView(context: context)
                    }
                )
            );
        }

        Widget _buildTopView() {
            return new Flexible(
                child: new Stack(
                    children: new List<Widget> {
                        new Positioned(
                            top: 0,
                            left: 0,
                            child: new CustomButton(
                                padding: EdgeInsets.symmetric(10, 16),
                                onPressed: () => this.widget.actionModel.mainRouterPop(),
                                child: new Icon(
                                    icon: Icons.close,
                                    size: 24,
                                    color: CColors.Icon
                                )
                            )
                        ),
                        new Align(
                            alignment: Alignment.center,
                            child: new Container(
                                height: 144,
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: new List<Widget> {
                                        new Container(
                                            child: new Icon(
                                                icon: Icons.UnityLogo,
                                                size: 80
                                            )
                                        ),
                                        new Text(
                                            "欢迎来到 Unity Connect",
                                            maxLines: 1,
                                            style: CTextStyle.H4
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildBottomView(BuildContext context) {
            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: new Column(
                    children: new List<Widget> {
                        this._buildWeChatButton(context: context),
                        new Container(height: 16),
                        new CustomButton(
                            onPressed: () => this.widget.actionModel.loginRouterPushToBindUnity(),
                            padding: EdgeInsets.zero,
                            child: new Container(
                                height: 48,
                                decoration: new BoxDecoration(
                                    color: CColors.White,
                                    border: Border.all(color: CColors.PrimaryBlue),
                                    borderRadius: BorderRadius.all(24)
                                ),
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget> {
                                        new Text(
                                            "使用 Unity ID 登录",
                                            maxLines: 1,
                                            style: CTextStyle.PLargeBlue
                                        )
                                    }
                                )
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(top: 16),
                            child: new RichText(
                                text: new TextSpan(
                                    children: new List<TextSpan> {
                                        new TextSpan(
                                            "登录代表您已经同意 ",
                                            style: CTextStyle.PSmallBody4
                                        ),
                                        new TextSpan(
                                            "用户协议",
                                            CTextStyle.PSmallBody4.copyWith(decoration: TextDecoration.underline),
                                            recognizer: new TapGestureRecognizer {
                                                onTap = () => this.widget.actionModel.openUrl(Config.termsOfService)
                                            }
                                        ),
                                        new TextSpan(
                                            " 和 ",
                                            style: CTextStyle.PSmallBody4
                                        ),
                                        new TextSpan(
                                            "隐私政策",
                                            CTextStyle.PSmallBody4.copyWith(decoration: TextDecoration.underline),
                                            recognizer: new TapGestureRecognizer {
                                                onTap = () => this.widget.actionModel.openUrl(Config.privacyPolicy)
                                            }
                                        )
                                    }
                                )
                            )
                        ),
                        new Container(
                            height: 16 + MediaQuery.of(context: context).padding.bottom
                        )
                    }
                )
            );
        }

        Widget _buildWeChatButton(BuildContext context) {
            if (!WechatPlugin.instance().isInstalled()) {
                return new Container();
            }

            WechatPlugin.instance().context = context;
            return new CustomButton(
                onPressed: () => {
                    WechatPlugin.instance(code => {
                            CustomDialogUtils.showCustomDialog(child: new CustomLoadingDialog());
                            this.widget.actionModel.loginByWechatAction(arg: code);
                        })
                        .login(Guid.NewGuid().ToString());
                },
                padding: EdgeInsets.zero,
                child: new Container(
                    height: 48,
                    decoration: new BoxDecoration(
                        color: CColors.PrimaryBlue,
                        borderRadius: BorderRadius.all(24)
                    ),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget> {
                            new Icon(
                                icon: Icons.WechatIcon,
                                size: 24,
                                color: CColors.White
                            ),
                            new Container(width: 8),
                            new Text(
                                "使用微信账号登录",
                                maxLines: 1,
                                style: CTextStyle.PLargeWhite
                            )
                        }
                    )
                )
            );
        }
    }
}