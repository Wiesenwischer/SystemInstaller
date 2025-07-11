<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('password','password-confirm'); section>
    <#if section = "header">
        ${msg("updatePasswordTitle")}
    <#elseif section = "form">
    <div id="kc-container">
        <div id="kc-container-wrapper">
            <!-- Left side - Reset Password Form -->
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
                        <h1>Reset Password</h1>
                        <p>Enter your new password below.</p>
                    </div>

                    <form id="kc-passwd-update-form" action="${url.loginAction}" method="post">
                        <div class="form-group">
                            <label for="password-new" class="${properties.kcLabelClass!}">
                                ${msg("passwordNew")} <span class="required">*</span>
                            </label>
                            <div class="password-field">
                                <input type="password" id="password-new" name="password-new" class="${properties.kcInputClass!}"
                                       autofocus autocomplete="new-password"
                                       aria-invalid="<#if messagesPerField.existsError('password','password-confirm')>true</#if>"
                                       placeholder="Enter new password" />
                                <button type="button" class="password-toggle" onclick="togglePassword('password-new')">
                                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" stroke="currentColor" stroke-width="2" fill="none"/>
                                        <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" fill="none"/>
                                    </svg>
                                </button>
                            </div>

                            <#if messagesPerField.existsError('password')>
                                <span id="input-error-password" class="${properties.kcInputErrorMessageClass!}" aria-live="polite">
                                    ${kcSanitize(messagesPerField.get('password'))?no_esc}
                                </span>
                            </#if>
                        </div>

                        <div class="form-group">
                            <label for="password-confirm" class="${properties.kcLabelClass!}">
                                ${msg("passwordConfirm")} <span class="required">*</span>
                            </label>
                            <div class="password-field">
                                <input type="password" id="password-confirm" name="password-confirm"
                                       class="${properties.kcInputClass!}"
                                       autocomplete="new-password"
                                       aria-invalid="<#if messagesPerField.existsError('password-confirm')>true</#if>"
                                       placeholder="Confirm new password" />
                                <button type="button" class="password-toggle" onclick="togglePassword('password-confirm')">
                                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" stroke="currentColor" stroke-width="2" fill="none"/>
                                        <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" fill="none"/>
                                    </svg>
                                </button>
                            </div>

                            <#if messagesPerField.existsError('password-confirm')>
                                <span id="input-error-password-confirm" class="${properties.kcInputErrorMessageClass!}" aria-live="polite">
                                    ${kcSanitize(messagesPerField.get('password-confirm'))?no_esc}
                                </span>
                            </#if>
                        </div>

                        <div class="${properties.kcFormGroupClass!}">
                            <div id="kc-form-buttons" class="${properties.kcFormButtonsClass!}">
                                <#if isAppInitiatedAction??>
                                    <input class="${properties.kcButtonClass!} ${properties.kcButtonPrimaryClass!} ${properties.kcButtonLargeClass!} btn-primary" type="submit" value="${msg("doSubmit")}" />
                                    <button class="${properties.kcButtonClass!} ${properties.kcButtonDefaultClass!} ${properties.kcButtonLargeClass!}" type="submit" name="cancel-aia" value="true" />${msg("doCancel")}</button>
                                <#else>
                                    <input class="${properties.kcButtonClass!} ${properties.kcButtonPrimaryClass!} ${properties.kcButtonBlockClass!} ${properties.kcButtonLargeClass!} btn-primary" type="submit" value="${msg("doSubmit")}" />
                                </#if>
                            </div>
                        </div>
                    </form>
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

    <script>
        function togglePassword(fieldId) {
            const passwordField = document.getElementById(fieldId);
            const icon = passwordField.nextElementSibling.querySelector('svg');
            
            if (passwordField.type === 'password') {
                passwordField.type = 'text';
                icon.innerHTML = '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" stroke="currentColor" stroke-width="2" fill="none"/><line x1="1" y1="1" x2="23" y2="23" stroke="currentColor" stroke-width="2"/>';
            } else {
                passwordField.type = 'password';
                icon.innerHTML = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" stroke="currentColor" stroke-width="2" fill="none"/><circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2" fill="none"/>';
            }
        }
    </script>
    </#if>

</@layout.registrationLayout>
