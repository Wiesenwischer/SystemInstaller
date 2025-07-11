<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('username'); section>
    <#if section = "header">
        ${msg("emailForgotTitle")}
    <#elseif section = "form">
    <div id="kc-container">
        <div id="kc-container-wrapper">
            <!-- Left side - Forgot Password Form -->
            <div class="login-form-container">
                <!-- Back to sign in link -->
                <a href="${url.loginUrl}" class="back-link">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M15 18L9 12L15 6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                    Back to sign in
                </a>

                <!-- Form content -->
                <div>
                    <!-- Header -->
                    <div class="login-header">
                        <h1>Forgot Password</h1>
                        <p>Enter your email address and we'll send you a link to reset your password.</p>
                    </div>

                    <form id="kc-reset-password-form" action="${url.loginAction}" method="post">
                        <div class="form-group">
                            <label for="username" class="${properties.kcLabelClass!}">
                                <#if !realm.loginWithEmailAllowed>${msg("username")}
                                <#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}
                                <#else>${msg("email")}</#if>
                                <span class="required">*</span>
                            </label>
                            <input type="text" id="username" name="username" class="${properties.kcInputClass!}" autofocus value="${(auth.attemptedUsername!'')}" aria-invalid="<#if messagesPerField.existsError('username')>true</#if>"
                                   placeholder="<#if !realm.loginWithEmailAllowed>${msg("username")}<#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}<#else>info@gmail.com</#if>" />
                            <#if messagesPerField.existsError('username')>
                                <span id="input-error-username" class="${properties.kcInputErrorMessageClass!}" aria-live="polite">
                                    ${kcSanitize(messagesPerField.get('username'))?no_esc}
                                </span>
                            </#if>
                        </div>
                        <div class="${properties.kcFormGroupClass!} ${properties.kcFormSettingClass!}">
                            <div id="kc-form-buttons" class="${properties.kcFormButtonsClass!}">
                                <input class="${properties.kcButtonClass!} ${properties.kcButtonPrimaryClass!} ${properties.kcButtonBlockClass!} ${properties.kcButtonLargeClass!} btn-primary" type="submit" value="${msg("doSubmit")}"/>
                            </div>
                        </div>
                    </form>

                    <div class="signup-link">
                        Remember your password? <a href="${url.loginUrl}">Back to Sign In</a>
                    </div>
                </div>
            </div>

            <!-- Right side - Branding -->
            <div class="login-branding-container">
                <div class="login-branding-content">
                    <h2>System Installer</h2>
                    <p>Professional Deployment Management Platform with Modern Authentication</p>
                </div>
            </div>
        </div>
    </div>
    </#if>

</@layout.registrationLayout>
