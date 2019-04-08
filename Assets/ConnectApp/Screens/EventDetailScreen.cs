using System;
using System.Collections.Generic;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Signers;
using ConnectApp.api;
using ConnectApp.canvas;
using ConnectApp.components;
using ConnectApp.components.pull_to_refresh;
using ConnectApp.constants;
using ConnectApp.models;
using ConnectApp.Models.Screen;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using ConnectApp.utils;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Config = ConnectApp.constants.Config;
using EventType = ConnectApp.models.EventType;

namespace ConnectApp.screens {
    public class EventDetailScreenConnector : StatelessWidget {
        public EventDetailScreenConnector(string eventId, EventType eventType) {
            this.eventId = eventId;
            this.eventType = eventType;
        }

        private readonly string eventId;
        private readonly EventType eventType;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, EventDetailScreenModel>(
                pure: true,
                converter: (state) => {
                    var channelId = state.eventState.channelId;
                    var channelMessageList = state.messageState.channelMessageList;
                    var messageList = new List<string>();
                    if (channelMessageList.ContainsKey(channelId))
                        messageList = channelMessageList[channelId];
                    return new EventDetailScreenModel {
                        eventId = eventId,
                        eventType = eventType,
                        currOldestMessageId = state.messageState.currOldestMessageId,
                        isLoggedIn = state.loginState.isLoggedIn,
                        eventDetailLoading = state.eventState.eventDetailLoading,
                        joinEventLoading = state.eventState.joinEventLoading,
                        showChatWindow = state.eventState.showChatWindow,
                        channelId = state.eventState.channelId,
                        messageList = messageList,
                        messageLoading = state.messageState.messageLoading,
                        hasMore = state.messageState.hasMore,
                        channelMessageDict = state.messageState.channelMessageDict,
                        eventsDict = state.eventState.eventsDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    return new EventDetailScreen(
                        viewModel,
                        mainRouterPop: () => dispatcher.dispatch(new MainNavigatorPopAction()),
                        pushToLogin: () => dispatcher.dispatch(new MainNavigatorPushToAction {
                            routeName = MainNavigatorRoutes.Login
                        }),
                        fetchEventDetail: (id) =>
                            dispatcher.dispatch<IPromise>(Actions.fetchEventDetail(id)),
                        joinEvent: (id) =>
                            dispatcher.dispatch(new JoinEventAction {eventId = id}),
                        sendMessage: (channelId, content, nonce, parentMessageId) => dispatcher.dispatch(
                            new SendMessageAction {
                                channelId = channelId,
                                content = content,
                                nonce = nonce,
                                parentMessageId = parentMessageId
                            }),
                        showChatWindow: (show) => dispatcher.dispatch(new ShowChatWindowAction {
                            show = show
                        }),
                        fetchMessages: (channelId, currOldestMessageId, isFirstLoad) => 
                            dispatcher.dispatch<IPromise>(
                                Actions.fetchMessages(channelId, currOldestMessageId, isFirstLoad)
                            ),
                        share: (type, title, description, linkUrl, imageUrl) => dispatcher.dispatch(new ShareAction {
                            type = type,
                            title = title,
                            description = description,
                            linkUrl = linkUrl,
                            imageUrl = imageUrl
                        })
                    );
                });
        }
    }
    
    public class EventDetailScreen : StatefulWidget {
        public EventDetailScreen(
            EventDetailScreenModel screenModel = null,
            Action mainRouterPop = null,
            Action pushToLogin = null,
            Func<string, IPromise> fetchEventDetail = null,
            Action<string> joinEvent = null,
            Action<bool> showChatWindow = null,
            Func<string, string, bool, IPromise> fetchMessages = null,
            Action<string, string, string, string> sendMessage = null,
            Action<ShareType, string, string, string, string> share = null,
            Key key = null
        ) : base(key) {
            this.screenModel = screenModel;
            this.mainRouterPop = mainRouterPop;
            this.pushToLogin = pushToLogin;
            this.fetchEventDetail = fetchEventDetail;
            this.joinEvent = joinEvent;
            this.showChatWindow = showChatWindow;
            this.fetchMessages = fetchMessages;
            this.sendMessage = sendMessage;
            this.share = share;
        }

        public readonly EventDetailScreenModel screenModel;
        public readonly Action mainRouterPop;
        public readonly Action pushToLogin;
        public readonly Func<string, IPromise> fetchEventDetail;
        public readonly Action<string> joinEvent;
        public readonly Action<bool> showChatWindow;
        public readonly Func<string, string, bool, IPromise> fetchMessages;
        public readonly Action<string, string, string, string> sendMessage;
        public readonly Action<ShareType, string, string, string, string> share;

        public override State createState() {
            return new _EventDetailScreenState();
        }
    }

    internal class _EventDetailScreenState : State<EventDetailScreen>, TickerProvider {
        private AnimationController _controller;
        private Animation<Offset> _position;
        private readonly TextEditingController _textController = new TextEditingController("");
        private readonly FocusNode _focusNode = new FocusNode();
        private readonly RefreshController _refreshController = new RefreshController();

        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, 300),
                vsync: this
            );
            widget.fetchEventDetail(widget.screenModel.eventId);
        }

        public override void dispose() {
            widget.showChatWindow(false);
            _textController.dispose();
            _controller.dispose();
            base.dispose();
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick, $"created by {this}");
        }

        private void _setAnimationPosition(BuildContext context) {
            if (_position != null) return;
            var screenHeight = MediaQuery.of(context).size.height;
            var screenWidth = MediaQuery.of(context).size.width;
            var ratio = 1.0f - 64.0f / (screenHeight - screenWidth * 9.0f / 16.0f);

            _position = new OffsetTween(
                new Offset(0, ratio),
                new Offset(0, 0)
            ).animate(new CurvedAnimation(
                _controller,
                Curves.easeInOut
            ));
        }


        void _onRefresh(bool up) {
            if (up) {
                widget.fetchMessages(widget.screenModel.channelId, widget.screenModel.currOldestMessageId, false)
                    .Then(() => _refreshController.sendBack(true, RefreshStatus.completed))
                    .Catch(_ => _refreshController.sendBack(true, RefreshStatus.failed));
            }
        }

        void _handleSubmitted(string text) {
            widget.sendMessage(widget.screenModel.channelId, text, Snowflake.CreateNonce(), "");
            _refreshController.scrollTo(0);
        }

        Widget _buildHeadTop(bool isShowShare,IEvent eventObj) {
            Widget shareWidget = new Container();
            if (isShowShare)
                shareWidget = new CustomButton(
                    child: new Icon(
                        Icons.share,
                        size: 28,
                        color: CColors.White
                    ),
                    onPressed: () => ShareUtils.showShareView(new ShareView(
                        onPressed: type =>
                        {
                            string linkUrl =
                                $"{Config.apiAddress}/events/{eventObj.id}";
                            string imageUrl = $"{eventObj.background}.200x0x1.jpg";
                            widget.share(type, eventObj.title, eventObj.shortDescription, linkUrl, imageUrl);
                        }))
                );
            return new Container(
                height: 44,
                padding: EdgeInsets.symmetric(horizontal: 8),
                decoration: new BoxDecoration(
                    gradient: new LinearGradient(
                        colors: new List<Color> {
                            new Color(0x80000000),
                            new Color(0x0)
                        },
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter
                    )
                ),
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new CustomButton(
                            onPressed: () => widget.mainRouterPop(),
                            child: new Icon(
                                Icons.arrow_back,
                                size: 28,
                                color: CColors.White
                            )
                        ),
                        shareWidget
                    }
                )
            );
        }

        Widget _buildEventHeader(IEvent eventObj, EventType eventType, EventStatus eventStatus,
            bool isLoggedIn) {
            return new Stack(
                children: new List<Widget> {
                    new EventHeader(eventObj, eventType, eventStatus, isLoggedIn),
                    new Positioned(
                        left: 0,
                        top: 0,
                        right: 0,
                        child: _buildHeadTop(eventType == EventType.onLine,eventObj = eventObj)
                    )
                }
            );
        }

        Widget _buildEventDetail(IEvent eventObj, EventType eventType, EventStatus eventStatus,
            bool isLoggedIn) {
            if (eventStatus != EventStatus.future && eventType == EventType.onLine && isLoggedIn)
                return new Expanded(
                    child: new Stack(
                        fit: StackFit.expand,
                        children: new List<Widget> {
                            new Container(
                                margin: EdgeInsets.only(bottom: 64),
                                color: CColors.White,
                                child: new EventDetail(eventObj)
                            ),
                            Positioned.fill(
                                new Container(
                                    child: new SlideTransition(
                                        position: _position,
                                        child: _buildChatWindow()
                                    )
                                )
                            )
                        }
                    )
                );
            return new Expanded(
                child: new EventDetail(eventObj)
            );
        }

        Widget _buildEventBottom(IEvent eventObj, EventType eventType, EventStatus eventStatus,
            bool isLoggedIn) {
            if (eventType == EventType.offline) return _buildOfflineRegisterNow(eventObj, isLoggedIn);
            if (eventStatus != EventStatus.future && eventType == EventType.onLine && isLoggedIn)
                return new Container();

            var onlineCount = eventObj.onlineMemberCount;
            var recordWatchCount = eventObj.recordWatchCount;
            var userIsCheckedIn = eventObj.userIsCheckedIn;
            var title = "";
            var subTitle = "";
            if (eventStatus == EventStatus.live) {
                title = "正在直播";
                subTitle = $"{onlineCount}人正在观看";
            }

            if (eventStatus == EventStatus.past) {
                title = "回放";
                subTitle = $"{recordWatchCount}次观看";
            }

            if (eventStatus == EventStatus.future || eventStatus == EventStatus.countDown) {
                var begin = eventObj.begin != null ? eventObj.begin : new TimeMap();
                var startTime = begin.startTime;
                if (startTime.isNotEmpty()) subTitle = DateConvert.GetFutureTimeFromNow(startTime);
                title = "距离开始还有";
            }
            
            var backgroundColor = CColors.PrimaryBlue;
            var joinInText = "立即加入";
            var textStyle = CTextStyle.PLargeMediumWhite;
            if (userIsCheckedIn && isLoggedIn) {
                backgroundColor = CColors.Disable;
                joinInText = "已加入";
                textStyle = CTextStyle.PLargeMediumWhite;
            }

            Widget child = new Text(
                joinInText,
                style: textStyle
            );
            if (widget.screenModel.joinEventLoading)
                child = new CustomActivityIndicator(
                    animationImage: AnimationImage.white
                );
            return new Container(
                height: 64,
                padding: EdgeInsets.symmetric(horizontal: 16),
                decoration: new BoxDecoration(
                    CColors.White,
                    border: new Border(new BorderSide(CColors.Separator))
                ),
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget> {
                                new Text(
                                    title,
                                    style: CTextStyle.PSmallBody4
                                ),
                                new Container(height: 2),
                                new Text(
                                    subTitle,
                                    style: CTextStyle.H5Body
                                ),
                                new CustomButton(
                                    onPressed: () => {
                                        if (widget.screenModel.joinEventLoading) return;
                                        if (!widget.screenModel.isLoggedIn) {
                                            widget.pushToLogin();
                                        }
                                        else {
                                            if (!userIsCheckedIn)
                                                widget.joinEvent(widget.screenModel.eventId);
                                        }
                                    },
                                    child: new Container(
                                        width: 96,
                                        height: 40,
                                        decoration: new BoxDecoration(
                                            backgroundColor,
                                            borderRadius: BorderRadius.all(4)
                                        ),
                                        alignment: Alignment.center,
                                        child: new Row(
                                            mainAxisAlignment: MainAxisAlignment.center,
                                            children: new List<Widget> {
                                                child
                                            }
                                        )
                                    )
                                )
                            }
                        )
                    }
                )
            );
        }

        Widget _buildChatWindow() {
            return new Container(
                child: new Column(
                    children: new List<Widget> {
                        _buildChatBar(widget.screenModel.showChatWindow),
                        _buildChatList(),
                        new CustomDivider(
                            height: 1,
                            color: CColors.Separator
                        ),
                        _buildTextField()
                    }
                )
            );
        }

        Widget _buildChatBar(bool showChatWindow) {
            IconData iconData;
            Widget bottomWidget;
            if (showChatWindow) {
                iconData = Icons.expand_more;
                bottomWidget = new Container();
            }
            else {
                iconData = Icons.expand_less;
                bottomWidget = new Text(
                    "轻点展开聊天",
                    style: CTextStyle.PSmallBody4
                );
            }

            return new GestureDetector(
                onTap: () => {
                    _focusNode.unfocus();
                    if (!showChatWindow)
                        _controller.forward();
                    else
                        _controller.reverse();
                    widget.showChatWindow(!widget.screenModel.showChatWindow);
                },
                child: new Container(
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    color: CColors.White,
                    height: showChatWindow ? 44 : 64,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    new Row(
                                        children: new List<Widget> {
                                            new Container(
                                                margin: EdgeInsets.only(right: 6),
                                                width: 6,
                                                height: 6,
                                                decoration: new BoxDecoration(
                                                    CColors.SecondaryPink,
                                                    borderRadius: BorderRadius.circular(3)
                                                )
                                            ),
                                            new Text(
                                                "直播聊天",
                                                style: new TextStyle(
                                                    height: 1.09f,
                                                    fontSize: 16,
                                                    fontFamily: "Roboto-Medium",
                                                    color: CColors.TextBody
                                                )
                                            )
                                        }
                                    ),
                                    bottomWidget
                                }
                            ),
                            new Icon(
                                iconData,
                                color: CColors.text3,
                                size: 28
                            )
                        }
                    )
                )
            );
        }

        Widget _buildChatList() {
            object child = new Container();
            if (widget.screenModel.messageLoading) 
                child = new GlobalLoading();
            else {
                if (widget.screenModel.messageList.Count <= 0) {
                    child = new BlankView("暂无聊天内容");
                }
                else {
                    child = new SmartRefresher(
                        controller: _refreshController,
                        enablePullDown: widget.screenModel.hasMore,
                        enablePullUp: false,
                        headerBuilder: (cxt, mode) => new SmartRefreshHeader(mode),
                        onRefresh: _onRefresh,
                        child: ListView.builder(
                            padding: EdgeInsets.only(16, right: 16, bottom: 10),
                            physics: new AlwaysScrollableScrollPhysics(),
                            itemCount: widget.screenModel.messageList.Count,
                            itemBuilder: (cxt, index) => new ChatMessage(
                                widget.screenModel.channelId,
                                widget.screenModel.messageList[widget.screenModel.messageList.Count - index - 1],
                                new ObjectKey(
                                    widget.screenModel.messageList[widget.screenModel.messageList.Count - index - 1])
                            )
                        )
                    );
                }
            }

            return new Flexible(
                child: new GestureDetector(
                    onTap: () => _focusNode.unfocus(),
                    child: new Container(
                        color: CColors.White,
                        child: (Widget)child
                    )
                )
            );
        }

        Widget _buildTextField() {
            var sendMessageLoading = false;
            return new Container(
                color: CColors.White,
                padding: EdgeInsets.symmetric(horizontal: 16),
                height: 40,
                child: new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new InputField(
                                // key: _textFieldKey,
                                controller: _textController,
                                focusNode: _focusNode,
                                height: 40,
                                style: new TextStyle(
                                    color: sendMessageLoading
                                        ? CColors.TextBody3
                                        : CColors.TextBody,
                                    fontFamily: "Roboto-Regular",
                                    fontSize: 16
                                ),
                                hintText: "说点想法…",
                                hintStyle: CTextStyle.PLargeBody4,
                                keyboardType: TextInputType.multiline,
                                maxLines: 1,
                                cursorColor: CColors.PrimaryBlue,
                                textInputAction: TextInputAction.send,
                                onSubmitted: _handleSubmitted
                            )
                        ),
                        sendMessageLoading
                            ? new Container(
                                width: 32,
                                height: 32,
                                child: new CustomActivityIndicator()
                            )
                            : new Container()
                    }
                )
            );
        }

        Widget _buildOfflineRegisterNow(IEvent eventObj, bool isLoggedIn) {
            var buttonText = "立即报名";
            var backgroundColor = CColors.PrimaryBlue;
            var isEnabled = false;
            if (eventObj.userIsCheckedIn && isLoggedIn) {
                buttonText = "已报名";
                backgroundColor = CColors.Disable;
                isEnabled = true;
            }

            return new Container(
                height: 64,
                padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                decoration: new BoxDecoration(
                    CColors.White,
                    border: new Border(new BorderSide(CColors.Separator))
                ),
                child: new CustomButton(
                    onPressed: () => {
                        if (isEnabled) return;
                        if (isLoggedIn)
                            widget.joinEvent(eventObj.id);
                        else
                            widget.pushToLogin();
                    },
                    padding: EdgeInsets.zero,
                    child: new Container(
                        decoration: new BoxDecoration(
                            backgroundColor,
                            borderRadius: BorderRadius.all(4)
                        ),
                        child: new Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: new List<Widget> {
                                new Text(
                                    buttonText,
                                    style: CTextStyle.PLargeMediumWhite
                                )
                            }
                        )
                    )
                )
            );
        }

        public override Widget build(BuildContext context) {
            _setAnimationPosition(context);
            var eventObj = new IEvent();
            if (widget.screenModel.eventsDict.ContainsKey(widget.screenModel.eventId)) 
                eventObj = widget.screenModel.eventsDict[widget.screenModel.eventId];
            if (widget.screenModel.eventDetailLoading || eventObj == null)
                return new EventDetailLoading();
            var eventStatus = DateConvert.GetEventStatus(eventObj.begin);

            return new Container(
                color: CColors.White,
                child: new SafeArea(
                    child: new Container(
                        color: CColors.White,
                        child: new Column(
                            children: new List<Widget> {
                                _buildEventHeader(eventObj, widget.screenModel.eventType, eventStatus, widget.screenModel.isLoggedIn),
                                _buildEventDetail(eventObj, widget.screenModel.eventType, eventStatus, widget.screenModel.isLoggedIn),
                                _buildEventBottom(eventObj, widget.screenModel.eventType, eventStatus, widget.screenModel.isLoggedIn)
                            }
                        )
                    )
                )
            );
        }
    }
}