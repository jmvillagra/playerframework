﻿using System;
using System.Linq;
using Microsoft.VideoAdvertising;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// A VPAID implementation base class for a web (HTML string) based ad.
    /// </summary>
    public class VpaidHtmlAdPlayer : VpaidWebAdPlayer
    {
        public VpaidHtmlAdPlayer(FlexibleOffset skippableOffset, TimeSpan? suggestedDuration, Uri clickThru, Size dimensions)
            : base(skippableOffset, suggestedDuration, clickThru, dimensions)
        { }

        public override void InitAd(double width, double height, string viewMode, int desiredBitrate, string creativeData, string environmentVariables)
        {
            base.InitAd(width, height, viewMode, desiredBitrate, creativeData, environmentVariables);
            base.WebView.NavigateToString(creativeData);
        }
    }

    /// <summary>
    /// A VPAID implementation base class for a web (IFrame URL) based ad.
    /// </summary>
    public class VpaidIFrameAdPlayer : VpaidWebAdPlayer
    {
        public VpaidIFrameAdPlayer(FlexibleOffset skippableOffset, TimeSpan? suggestedDuration, Uri clickThru, Size dimensions)
            : base(skippableOffset, suggestedDuration, clickThru, dimensions)
        { }

        public override void InitAd(double width, double height, string viewMode, int desiredBitrate, string creativeData, string environmentVariables)
        {
            base.InitAd(width, height, viewMode, desiredBitrate, creativeData, environmentVariables);
            base.WebView.Navigate(new Uri(creativeData));
        }
    }

    /// <summary>
    /// A VPAID implementation base class for a web (HTML) based ad.
    /// </summary>
    public abstract class VpaidWebAdPlayer : AdHost, IVpaid2
    {
        readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
        readonly MarkerHelper markerHelper = new MarkerHelper();
        protected WebView WebView { get; private set; }

        /// <summary>
        /// Gets the position in the ad at which the ad can be skipped. If null, the ad cannot be skipped.
        /// </summary>
        public FlexibleOffset SkippableOffset { get; private set; }

        /// <summary>
        /// Gets the duration of the ad. If not specified, the ad is closed when the next ad is played.
        /// </summary>
        public TimeSpan? SuggestedDuration { get; private set; }

        /// <summary>
        /// Gets the Uri to navigate to when the ad is clicked or tapped. Can be null of no action should take place.
        /// </summary>
        public Uri ClickThru { get; private set; }

        const string Marker_SkippableOffset = "SkippableOffset";
        const string Marker_FirstQuartile = "FirstQuartile";
        const string Marker_Midpoint = "Midpoint";
        const string Marker_ThirdQuartile = "ThirdQuartile";
        const string Marker_DurationReached = "DurationReached";

        /// <summary>
        /// Creates a new instance of VpaidImageAdPlayer.
        /// </summary>
        /// <param name="skippableOffset">The position in the ad at which the ad can be skipped. If null, the ad cannot be skipped.</param>
        /// <param name="suggestedDuration">The duration of the ad. If not specified, the ad is closed when the next ad is played.</param>
        /// <param name="clickThru">The Uri to navigate to when the ad is clicked or tapped. Can be null of no action should take place.</param>
        /// <param name="dimensions">The dimensions of the ad.</param>
        public VpaidWebAdPlayer(FlexibleOffset skippableOffset, TimeSpan? suggestedDuration, Uri clickThru, Size dimensions)
        {
            IsHitTestVisible = false;
            WebView = new WebView();
            WebView.Visibility = Visibility.Collapsed;
            Background = new SolidColorBrush(Colors.Transparent);
            State = AdState.None;
            AdLinear = false;
            InitialDimensions = dimensions;

            SkippableOffset = skippableOffset;
            SuggestedDuration = suggestedDuration;
            ClickThru = clickThru;
            this.NavigateUri = ClickThru;
        }

        /// <inheritdoc />
        public string HandshakeVersion(string version)
        {
            if (version.StartsWith("1."))
            {
                return version;
            }
            else
            {
                return "2.0";   // return the highest version of VPAID that we support
            }
        }

        /// <inheritdoc />
        public virtual void InitAd(double width, double height, string viewMode, int desiredBitrate, string creativeData, string environmentVariables)
        {
            this.Content = WebView;
            WebView.NavigationCompleted += WebView_NavigationCompleted;
            WebView.LoadCompleted += WebView_LoadCompleted;
            WebView.NavigationFailed += WebView_NavigationFailed;
            State = AdState.Loading;
            adSkippableState = false;
        }

        void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                OnNavigationSuccess();
            }
            else
            {
                OnNavigationFailed(e.WebErrorStatus.ToString());
            }
        }

        void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            var error = e.WebErrorStatus.ToString();
            OnNavigationFailed(error);
        }

        void WebView_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            OnNavigationSuccess();
        }

        void OnNavigationFailed(string error)
        {
            if (State != AdState.Complete && State != AdState.Failed)
            {
                State = AdState.Failed;
                Teardown();
                var args = new VpaidMessageEventArgs();
                args.Message = error;
                if (AdError != null) AdError(this, args);
            }
        }

        void OnNavigationSuccess()
        {
            State = AdState.Loaded;
            if (AdLoaded != null) AdLoaded(this, EventArgs.Empty);

            if (SkippableOffset != null)
            {
                // create a marker for the skippable offset
                TimelineMarker skippableOffsetMarker = null;
                if (!SkippableOffset.IsAbsolute)
                {
                    skippableOffsetMarker = new TimelineMarker() { Time = TimeSpan.FromSeconds(AdDuration.TotalSeconds * SkippableOffset.RelativeOffset) };
                }
                else
                {
                    skippableOffsetMarker = new TimelineMarker() { Time = SkippableOffset.AbsoluteOffset };
                }
                if (skippableOffsetMarker != null)
                {
                    skippableOffsetMarker.Type = Marker_SkippableOffset;
                    markerHelper.Markers.Add(skippableOffsetMarker);
                }
            }

            if (SuggestedDuration.HasValue)
            {
                // create markers for the quartile events
                markerHelper.Markers.Add(new TimelineMarker() { Type = Marker_FirstQuartile, Time = TimeSpan.FromSeconds(AdDuration.TotalSeconds * .25) });
                markerHelper.Markers.Add(new TimelineMarker() { Type = Marker_Midpoint, Time = TimeSpan.FromSeconds(AdDuration.TotalSeconds * .5) });
                markerHelper.Markers.Add(new TimelineMarker() { Type = Marker_ThirdQuartile, Time = TimeSpan.FromSeconds(AdDuration.TotalSeconds * .75) });
                // create marker for duration
                markerHelper.Markers.Add(new TimelineMarker() { Type = Marker_DurationReached, Time = AdDuration });
            }

            markerHelper.MarkerReached += markerHelper_MarkerReached;
        }
        void timer_Tick(object sender, object e)
        {
            if (AdRemainingTimeChange != null) AdRemainingTimeChange(this, EventArgs.Empty);
        }

        void markerHelper_MarkerReached(object sender, MarkerReachedEventArgs e)
        {
            switch (e.Marker.Type)
            {
                case Marker_SkippableOffset:
                    AdSkippableState = true;
                    markerHelper.Markers.Remove(e.Marker);
                    break;
                case Marker_FirstQuartile:
                    if (AdVideoFirstQuartile != null) AdVideoFirstQuartile(this, EventArgs.Empty);
                    markerHelper.Markers.Remove(e.Marker);
                    break;
                case Marker_Midpoint:
                    if (AdVideoMidpoint != null) AdVideoMidpoint(this, EventArgs.Empty);
                    markerHelper.Markers.Remove(e.Marker);
                    break;
                case Marker_ThirdQuartile:
                    if (AdVideoThirdQuartile != null) AdVideoThirdQuartile(this, EventArgs.Empty);
                    markerHelper.Markers.Remove(e.Marker);
                    break;
                case Marker_DurationReached:
                    if (AdVideoComplete != null) AdVideoComplete(this, EventArgs.Empty);
                    markerHelper.Markers.Remove(e.Marker);
                    StopAd();
                    break;
            }
        }

        /// <inheritdoc />
        public void StartAd()
        {
            State = AdState.Starting;
            markerHelper.Start();

            IsHitTestVisible = true;
            WebView.Visibility = Visibility.Visible; // WebView.Opacity does nothing, set visibility instead

            this.Navigated += AdPlayer_Navigated;
            this.SizeChanged += AdPlayer_SizeChanged;
            timer.Tick += timer_Tick;

            State = AdState.Playing;
            if (AdStarted != null) AdStarted(this, EventArgs.Empty);
            if (AdImpression != null) AdImpression(this, EventArgs.Empty);
            if (SuggestedDuration.HasValue)
            {
                timer.Start();
                if (AdVideoStart != null) AdVideoStart(this, EventArgs.Empty);
            }
        }

        void AdPlayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AdSizeChanged != null) AdSizeChanged(this, EventArgs.Empty);
        }

        void AdPlayer_Navigated(object sender, RoutedEventArgs e)
        {
            var clickEventArgs = new ClickThroughEventArgs() { Url = ClickThru.OriginalString };
            clickEventArgs.PlayerHandles = !(e.OriginalSource is HyperlinkButton);
            if (AdClickThru != null) AdClickThru(this, clickEventArgs);
        }

        /// <inheritdoc />
        public void StopAd()
        {
            OnAdEnding();
        }

        /// <summary>
        /// Called when the ad is ending for any reason besides a failure.
        /// </summary>
        protected void OnAdEnding()
        {
            if (State != AdState.Complete && State != AdState.Failed)
            {
                State = AdState.Complete;
                Teardown();
                if (AdStopped != null) AdStopped(this, EventArgs.Empty);
            }
        }

        private void Teardown()
        {
            this.Navigated -= AdPlayer_Navigated;
            this.SizeChanged -= AdPlayer_SizeChanged;
            timer.Tick -= timer_Tick;
            if (timer.IsEnabled) timer.Stop();
            WebView.NavigationCompleted -= WebView_NavigationCompleted;
            WebView.LoadCompleted -= WebView_LoadCompleted;
            WebView.NavigationFailed -= WebView_NavigationFailed;
            this.Content = null;
            markerHelper.Stop();
            Opacity = 0;
        }

        /// <inheritdoc />
        public void ResizeAd(double width, double height, string viewMode)
        {
            // normally we don't have to do anything since we rely on Xaml scaling to resize us automatically.
        }

        /// <inheritdoc />
        public void PauseAd()
        {
            markerHelper.Stop();
            State = AdState.Paused;
            if (AdPaused != null) AdPaused(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void ResumeAd()
        {
            markerHelper.Resume();
            State = AdState.Playing;
            if (AdPlaying != null) AdPlaying(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void ExpandAd()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CollapseAd()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool AdExpanded
        {
            get { return true; }
        }

        /// <inheritdoc />
        public TimeSpan AdRemainingTime
        {
            get
            {
                return AdDuration == TimeSpan.Zero ? TimeSpan.Zero : AdDuration.Subtract(markerHelper.Position);
            }
        }

        double adVolume;
        /// <inheritdoc />
        public double AdVolume
        {
            get
            {
                return adVolume;
            }
            set
            {
                adVolume = value;
                if (AdVolumeChanged != null) AdVolumeChanged(this, EventArgs.Empty);
            }
        }

#pragma warning disable 0067

        /// <inheritdoc />
        public event EventHandler<object> AdLoaded;
        /// <inheritdoc />
        public event EventHandler<object> AdStarted;
        /// <inheritdoc />
        public event EventHandler<object> AdStopped;
        /// <inheritdoc />
        public event EventHandler<object> AdPaused;
        /// <inheritdoc />
        public event EventHandler<object> AdPlaying;
        /// <inheritdoc />
        public event EventHandler<object> AdExpandedChanged;
        /// <inheritdoc />
        public event EventHandler<object> AdLinearChanged;
        /// <inheritdoc />
        public event EventHandler<object> AdVolumeChanged;
        /// <inheritdoc />
        public event EventHandler<object> AdVideoStart;
        /// <inheritdoc />
        public event EventHandler<object> AdVideoFirstQuartile;
        /// <inheritdoc />
        public event EventHandler<object> AdVideoMidpoint;
        /// <inheritdoc />
        public event EventHandler<object> AdVideoThirdQuartile;
        /// <inheritdoc />
        public event EventHandler<object> AdVideoComplete;
        /// <inheritdoc />
        public event EventHandler<object> AdUserAcceptInvitation;
        /// <inheritdoc />
        public event EventHandler<object> AdUserClose;
        /// <inheritdoc />
        public event EventHandler<object> AdUserMinimize;
        /// <inheritdoc />
        public event EventHandler<object> AdRemainingTimeChange;
        /// <inheritdoc />
        public event EventHandler<object> AdImpression;
        /// <inheritdoc />
        public event EventHandler<ClickThroughEventArgs> AdClickThru;
        /// <inheritdoc />
        public event EventHandler<VpaidMessageEventArgs> AdError;
        /// <inheritdoc />
        public event EventHandler<VpaidMessageEventArgs> AdLog;

        // VPAID 2.0
        /// <inheritdoc />
        public event EventHandler<object> AdSkipped;
        /// <inheritdoc />
        public event EventHandler<object> AdSizeChanged;
        /// <inheritdoc />
        public event EventHandler<object> AdSkippableStateChange;
        /// <inheritdoc />
        public event EventHandler<object> AdDurationChange;
        /// <inheritdoc />
        public event EventHandler<AdInteractionEventArgs> AdInteraction; // TODO: raise this event once VPAID 2.0 defines the correct IDs to pass.

#pragma warning restore 0067

        private AdState State { get; set; }

        private enum AdState
        {
            None,
            Loading,
            Loaded,
            Starting,
            Playing,
            Paused,
            Complete,
            Failed
        }

        /// <inheritdoc />
        public void SkipAd()
        {
            if (AdSkippableState)
            {
                if (AdSkipped != null) AdSkipped(this, EventArgs.Empty);
                OnAdEnding();
            }
        }

        /// <inheritdoc />
        public double AdWidth
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public double AdHeight
        {
            get { return 0; }
        }

        bool adSkippableState;
        /// <inheritdoc />
        public bool AdSkippableState
        {
            get { return adSkippableState; }
            protected set
            {
                if (adSkippableState != value)
                {
                    adSkippableState = value;
                    if (AdSkippableStateChange != null) AdSkippableStateChange(this, EventArgs.Empty);
                }
            }
        }

        static TimeSpan DefaultLinearDuration = TimeSpan.FromSeconds(10);
        /// <inheritdoc />
        public TimeSpan AdDuration
        {
            get
            {
                return SuggestedDuration.GetValueOrDefault(TimeSpan.Zero);
            }
        }

        /// <inheritdoc />
        public string AdCompanions
        {
            get { return string.Empty; }
        }

        /// <inheritdoc />
        public bool AdIcons
        {
            get { return false; }
        }
    }
}