<#import "template.ftl" as layout>
<@layout.registrationLayout; section>
    <#if section = "header">
        ${msg("loginAccountTitle")}
    <#elseif section = "form">
    <div id="kc-container">
        <div id="kc-container-wrapper">
            <!-- Left side - Login Form -->
            <div class="login-form-container">
                <!-- Form content -->
                <div>
                    <!-- Header -->
                    <div class="login-header">
                        <h1>Sign In</h1>
                        <p>Enter your email and password to sign in!</p>
                    </div>

                    <!-- Social login buttons -->
                    <#if realm.password && social.providers??>
                        <div class="social-buttons">
                            <#list social.providers as p>
                                <#if p.alias == "google">
                                    <a id="social-${p.alias}" class="social-button" type="button" href="${p.loginUrl}">
                                        <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                                            <path d="M18.7511 10.1944C18.7511 9.47495 18.6915 8.94995 18.5626 8.40552H10.1797V11.6527H15.1003C15.0011 12.4597 14.4654 13.675 13.2749 14.4916L13.2582 14.6003L15.9087 16.6126L16.0924 16.6305C17.7788 15.1041 18.7511 12.8583 18.7511 10.1944Z" fill="#4285F4"/>
                                            <path d="M10.1788 18.75C12.5895 18.75 14.6133 17.9722 16.0915 16.6305L13.274 14.4916C12.5201 15.0068 11.5081 15.3666 10.1788 15.3666C7.81773 15.3666 5.81379 13.8402 5.09944 11.7305L4.99473 11.7392L2.23868 13.8295L2.20264 13.9277C3.67087 16.786 6.68674 18.75 10.1788 18.75Z" fill="#34A853"/>
                                            <path d="M5.10014 11.7305C4.91165 11.186 4.80257 10.6027 4.80257 9.99992C4.80257 9.3971 4.91165 8.81379 5.09022 8.26935L5.08523 8.1534L2.29464 6.02954L2.20333 6.0721C1.5982 7.25823 1.25098 8.5902 1.25098 9.99992C1.25098 11.4096 1.5982 12.7415 2.20333 13.9277L5.10014 11.7305Z" fill="#FBBC05"/>
                                            <path d="M10.1789 4.63331C11.8554 4.63331 12.9864 5.34303 13.6312 5.93612L16.1511 3.525C14.6035 2.11528 12.5895 1.25 10.1789 1.25C6.68676 1.25 3.67088 3.21387 2.20264 6.07218L5.08953 8.26943C5.81381 6.15972 7.81776 4.63331 10.1789 4.63331Z" fill="#EB4335"/>
                                        </svg>
                                        Sign in with Google
                                    </a>
                                <#elseif p.alias == "github">
                                    <a id="social-${p.alias}" class="social-button" type="button" href="${p.loginUrl}">
                                        <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                                            <path d="M12 0C5.374 0 0 5.373 0 12 0 17.302 3.438 21.8 8.207 23.387c.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23A11.509 11.509 0 0112 5.803c1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576C20.566 21.797 24 17.3 24 12c0-6.627-5.373-12-12-12z"/>
                                        </svg>
                                        Sign in with GitHub
                                    </a>
                                <#else>
                                    <a id="social-${p.alias}" class="social-button" type="button" href="${p.loginUrl}">
                                        <#if p.iconClasses?has_content>
                                            <i class="${p.iconClasses}" aria-hidden="true"></i>
                                        </#if>
                                        <span>${p.displayName!}</span>
                                    </a>
                                </#if>
                            </#list>
                        </div>

                        <!-- Divider -->
                        <div class="divider">
                            <span>Or</span>
                        </div>
                    </#if>

                    <!-- Messages -->
                    <#if message?has_content && (message.type != 'warning' || !isAppInitiatedAction??)>
                        <div class="alert alert-${message.type}">
                            <span class="kc-feedback-text">${kcSanitize(message.summary)?no_esc}</span>
                        </div>
                    </#if>

                    <!-- Login form -->
                    <#if realm.password>
                        <form id="kc-form-login" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post">
                            <div class="form-group">
                                <label for="username">
                                    <#if !realm.loginWithEmailAllowed>${msg("username")}
                                    <#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}
                                    <#else>${msg("email")}</#if>
                                    <span class="required">*</span>
                                </label>

                                <#if usernameEditDisabled??>
                                    <input tabindex="1" id="username" name="username" value="${(login.username!'')}" type="text" disabled placeholder="<#if !realm.loginWithEmailAllowed>${msg("username")}<#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}<#else>${msg("email")}</#if>" />
                                <#else>
                                    <input tabindex="1" id="username" name="username" value="${(login.username!'')}" type="text" autofocus autocomplete="off"
                                           aria-invalid="<#if messagesPerField.existsError('username')>true</#if>"
                                           placeholder="<#if !realm.loginWithEmailAllowed>${msg("username")}<#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}<#else>info@gmail.com</#if>" />

                                    <#if messagesPerField.existsError('username')>
                                        <span id="input-error-username" aria-live="polite">
                                                    ${kcSanitize(messagesPerField.get('username'))?no_esc}
                                        </span>
                                    </#if>
                                </#if>
                            </div>

                            <div class="form-group">
                                <label for="password">
                                    ${msg("password")} <span class="required">*</span>
                                </label>

                                <div class="password-field">
                                    <input tabindex="2" id="password" name="password" type="password" autocomplete="off"
                                           aria-invalid="<#if messagesPerField.existsError('password')>true</#if>"
                                           placeholder="Enter your password" />
                                    <button type="button" class="password-toggle" onclick="togglePassword()">
                                        <svg id="password-show" width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" stroke="currentColor" stroke-width="2" fill="none"/>
                                            <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" fill="none"/>
                                        </svg>
                                        <svg id="password-hide" width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" style="display: none;">
                                            <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" stroke="currentColor" stroke-width="2" fill="none"/>
                                            <line x1="1" y1="1" x2="23" y2="23" stroke="currentColor" stroke-width="2"/>
                                        </svg>
                                    </button>
                                </div>

                                <#if messagesPerField.existsError('password')>
                                    <span id="input-error-password" aria-live="polite">
                                                ${kcSanitize(messagesPerField.get('password'))?no_esc}
                                    </span>
                                </#if>
                            </div>

                            <div class="form-options">
                                <div class="checkbox-group">
                                    <#if realm.rememberMe && !usernameEditDisabled??>
                                        <input tabindex="3" id="rememberMe" name="rememberMe" type="checkbox"
                                               <#if login.rememberMe??>checked</#if> />
                                        <label for="rememberMe">Keep me logged in</label>
                                    </#if>
                                </div>
                                <#if realm.resetPasswordAllowed>
                                    <a tabindex="5" href="${url.loginResetCredentialsUrl}" class="forgot-password">Forgot password?</a>
                                </#if>
                            </div>

                            <div class="form-group">
                                <input type="hidden" id="id-hidden-input" name="credentialId" <#if auth.selectedCredential?has_content>value="${auth.selectedCredential}"</#if>/>
                                <button tabindex="4" class="btn-primary" name="login" id="kc-login" type="submit">
                                    <span>${msg("doLogIn")}</span>
                                </button>
                            </div>
                        </form>
                    </#if>

                    <!-- Sign up link -->
                    <#if realm.password && realm.registrationAllowed && !registrationDisabled??>
                        <div class="signup-link">
                            Don't have an account? <a tabindex="6" href="${url.registrationUrl}">Sign Up</a>
                        </div>
                    </#if>
                </div>
            </div>

            <!-- Right side - Branding -->
            <div class="login-branding-container">
                <div class="login-branding-content">
                    <#if realm.displayName?has_content>
                        <h2>${realm.displayName}</h2>
                    <#else>
                        <h2>System Installer</h2>
                    </#if>
                    <p>Professional Deployment Management Platform with Modern Authentication</p>
                </div>
            </div>
        </div>
    </div>

    <script>
        function togglePassword() {
            const passwordField = document.getElementById('password');
            const showIcon = document.getElementById('password-show');
            const hideIcon = document.getElementById('password-hide');
            
            if (passwordField.type === 'password') {
                passwordField.type = 'text';
                showIcon.style.display = 'none';
                hideIcon.style.display = 'block';
            } else {
                passwordField.type = 'password';
                showIcon.style.display = 'block';
                hideIcon.style.display = 'none';
            }
        }
    </script>
    </#if>

</@layout.registrationLayout>
