﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Facebook.Client.Controls
{
    /// <summary>
    /// Represents a button control that can log in or log out the user when clicked.
    /// </summary>
    /// <remarks>
    /// The LoginView keeps track of the authentication status and shows an appropriate label that 
    /// reflects whether the user is currently authenticated. When a user logs in, it can automatically 
    /// retrieve their basic information.
    /// </remarks>
    public sealed class LoginView : Control
    {
        private Button loginButton;
        private FacebookSessionClient facebookSessionClient;

        /// <summary>
        /// Initializes a new instance of the LoginView class. 
        /// </summary>
        public LoginView()
        {
            this.DefaultStyleKey = typeof(LoginView);
        }

        #region Part Definitions

        private const string Part_LoginButton = "PART_LoginButton";

        #endregion Part Definitions

        #region Events

        /// <summary>
        /// Occurs whenever the status of the session associated with this control changes.
        /// </summary>
        public event EventHandler<SessionStateChangedEventArgs> SessionStateChanged;

        /// <summary>
        /// Occurs whenever a communication or authentication error occurs while logging in.
        /// </summary>
        public event EventHandler<AuthenticationErrorEventArgs> AuthenticationError;

        /// <summary>
        /// Occurs whenever the current user changes.
        /// </summary>
        /// <remarks>
        /// To retrieve the current user information, the FetchUserInfo property must be set to true.
        /// </remarks>
        public event EventHandler<UserInfoChangedEventArgs> UserInfoChanged;

        #endregion Events

        #region Properties

        #region ApplicationId

        /// <summary>
        /// Gets or sets the application ID to be used to open the session.
        /// </summary>
        public string ApplicationId
        {
            get { return (string)GetValue(ApplicationIdProperty); }
            set { SetValue(ApplicationIdProperty, value); }
        }

        /// <summary>
        /// Identifies the ApplicationId dependency property.
        /// </summary>
        public static readonly DependencyProperty ApplicationIdProperty =
            DependencyProperty.Register("ApplicationId", typeof(string), typeof(LoginView), new PropertyMetadata(string.Empty, OnApplicationIdPropertyChanged));

        private static void OnApplicationIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (LoginView)d;
            target.facebookSessionClient = new FacebookSessionClient(target.ApplicationId);
        }

        #endregion ApplicationId

        #region DefaultAudience

        /// <summary>
        /// The default audience to use, if publish permissions are requested at login time.
        /// </summary>
        /// <remarks>
        /// Certain operations such as publishing a status or publishing a photo require an audience. When the user grants an application 
        /// permission to perform a publish operation, a default audience is selected as the publication ceiling for the application. This 
        /// enumerated value allows the application to select which audience to ask the user to grant publish permission for.
        /// </remarks>
        public DefaultAudience DefaultAudience
        {
            get { return (DefaultAudience)GetValue(DefaultAudienceProperty); }
            set { SetValue(DefaultAudienceProperty, value); }
        }

        /// <summary>
        /// Identifies the DefaultAudience dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultAudienceProperty =
            DependencyProperty.Register("DefaultAudience", typeof(DefaultAudience), typeof(LoginView), new PropertyMetadata(DefaultAudience.None));

        #endregion DefaultAudience

        #region ReadPermissions

        /// <summary>
        /// The read permissions to request.
        /// </summary>
        /// <remarks>
        /// Note, that if read permissions are specified, then publish permissions should not be specified.
        /// </remarks>
        public string ReadPermissions
        {
            get { return (string)GetValue(ReadPermissionsProperty); }
            set { SetValue(ReadPermissionsProperty, value); }
        }

        /// <summary>
        /// Identifies the ReadPermissions dependency property.
        /// </summary>
        public static readonly DependencyProperty ReadPermissionsProperty =
            DependencyProperty.Register("ReadPermissions", typeof(string), typeof(LoginView), new PropertyMetadata(null));
        
        #endregion ReadPermissions

        #region PublishPermissions

        /// <summary>
        /// The publish permissions to request.
        /// </summary>
        /// <remarks>
        /// Note, that a defaultAudience value of OnlyMe, Everyone, or Friends should be set if publish permissions are 
        /// specified. Additionally, when publish permissions are specified, then read should not be specified.
        /// </remarks>
        public string PublishPermissions
        {
            get { return (string)GetValue(PublishPermissionsProperty); }
            set { SetValue(PublishPermissionsProperty, value); }
        }

        /// <summary>
        /// Identifies the PublishPermissions dependency property.
        /// </summary>
        public static readonly DependencyProperty PublishPermissionsProperty =
            DependencyProperty.Register("PublishPermissions", typeof(string), typeof(LoginView), new PropertyMetadata(null));

        #endregion PublishPermissions

        #region FetchUserInfo

        /// <summary>
        /// Controls whether the user information is fetched when the session is opened. Default is true.
        /// </summary>
        public bool FetchUserInfo
        {
            get { return (bool)GetValue(FetchUserInfoProperty); }
            set { SetValue(FetchUserInfoProperty, value); }
        }

        /// <summary>
        /// Identifies the FetchUserInfo dependency property.
        /// </summary>
        public static readonly DependencyProperty FetchUserInfoProperty =
            DependencyProperty.Register("FetchUserInfo", typeof(bool), typeof(LoginView), new PropertyMetadata(true));
        
        #endregion FetchUserInfo

        #region CurrentSession

        /// <summary>
        /// Gets the current active session.
        /// </summary>
        public FacebookSession CurrentSession
        {
            get { return (FacebookSession)GetValue(CurrentSessionProperty); }
            private set { SetValue(CurrentSessionProperty, value); }
        }

        /// <summary>
        /// Identifies the CurrentSession dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentSessionProperty =
            DependencyProperty.Register("CurrentSession", typeof(FacebookSession), typeof(LoginView), new PropertyMetadata(null, OnCurrentSessionPropertyChanged));

        private static void OnCurrentSessionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (LoginView)d;
            target.SetLoginButtonLabel();
        }
        
        #endregion CurrentSession

        #region Label

        /// <summary>
        /// Gets or sets the label shown by the control.
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Identifies the Label dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(LoginView), new PropertyMetadata(0));
        
        #endregion Label

        #endregion Properties

        #region Implementation

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate. In simplest 
        /// terms, this means the method is called just before a UI element displays in your app. Override this method to influence the 
        /// default post-template logic of a class. 
        /// </summary>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.loginButton = this.GetTemplateChild(Part_LoginButton) as Button;
            if (this.loginButton == null)
            {
                // TODO: throw appropriate exception
                throw new Exception(string.Format("Template element '{0}' is missing.", Part_LoginButton));
            }

            this.loginButton.Click += OnLoginButtonClicked;
            SetLoginButtonLabel();
        }

        async void OnLoginButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.CurrentSession == null)
            {
                await LogIn();
            }
            else
            {
                LogOut();
            }
        }

        private async Task LogIn()
        {
            try
            {
                RaiseSessionStateChanged(new SessionStateChangedEventArgs(FacebookSessionState.Opening));

                // TODO: using only ReadPermissions for the time being until we decide how 
                // to handle separate ReadPermissions and PublishPermissions
                var session = await this.facebookSessionClient.LoginAsync(this.ReadPermissions);

                // initialize current session
                this.CurrentSession = session;
                RaiseSessionStateChanged(new SessionStateChangedEventArgs(FacebookSessionState.Opened));

                // retrieve information about the current user
                if (this.FetchUserInfo)
                {
                    // TODO: implement fetching user info
                    var userInfo = new UserInfoChangedEventArgs(
                        new FacebookUser()
                        {
                            Id = session.FacebookId,
                            Name = "Name placeholder",
                            UserName = "UserName placeholder",
                            FirstName = "FirstName placeholder",
                            MiddleName = "MiddleName placeholder",
                            LastName = "LastName placeholder",
                            Birthday = "Birthday placeholder",
                            Link = "Link placeholder"
                        });

                    RaiseUserInfoChanged(userInfo);
                }
            }
            catch (InvalidOperationException error)
            {
                // TODO: need to obtain richer information than a generic InvalidOperationException
                var authenticationErrorEventArgs =
                    new AuthenticationErrorEventArgs("Login failure.", error.Message);

                RaiseAuthenticationFailure(authenticationErrorEventArgs);
            }
        }

        private void LogOut()
        {
            this.facebookSessionClient.Logout();
            this.CurrentSession = null;
            RaiseSessionStateChanged(new SessionStateChangedEventArgs(FacebookSessionState.Closed));
        }

        private void RaiseSessionStateChanged(SessionStateChangedEventArgs e)
        {
            EventHandler<SessionStateChangedEventArgs> handler = SessionStateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RaiseUserInfoChanged(UserInfoChangedEventArgs e)
        {
            EventHandler<UserInfoChangedEventArgs> handler = UserInfoChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void RaiseAuthenticationFailure(AuthenticationErrorEventArgs e)
        {
            EventHandler<AuthenticationErrorEventArgs> handler = AuthenticationError;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void SetLoginButtonLabel()
        {
            this.loginButton.Content = this.CurrentSession == null ? "Log In" : "Log Out";
        }

        #endregion Implementation
    }
}
