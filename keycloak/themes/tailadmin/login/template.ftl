<#macro registrationLayout bodyClass="" displayInfo=false displayMessage=true displayRequiredFields=false>
<!DOCTYPE html>
<html class="${properties.kcHtmlClass!}"<#if realm.internationalizationEnabled> lang="${locale.current}"</#if>>

<head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="robots" content="noindex, nofollow">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <#if properties.meta?has_content>
        <#list properties.meta?split(' ') as meta>
            <meta name="${meta?split('==')[0]}" content="${meta?split('==')[1]}"/>
        </#list>
    </#if>
    <title>ReadyStackGo - ${msg("loginTitle",(realm.displayName!'ReadyStackGo'))}</title>
    <link rel="icon" href="${url.resourcesPath}/img/favicon.ico" />
    <#if properties.stylesCommon?has_content>
        <#list properties.stylesCommon?split(' ') as style>
            <link href="${url.resourcesCommonPath}/${style}" rel="stylesheet" />
        </#list>
    </#if>
    <#if properties.styles?has_content>
        <#list properties.styles?split(' ') as style>
            <link href="${url.resourcesPath}/${style}" rel="stylesheet" />
        </#list>
    </#if>
    <#if properties.scripts?has_content>
        <#list properties.scripts?split(' ') as script>
            <script src="${url.resourcesPath}/${script}" type="text/javascript"></script>
        </#list>
    </#if>
    <#if scripts??>
        <#list scripts as script>
            <script src="${script}" type="text/javascript"></script>
        </#list>
    </#if>
    <script type="module">
        import { registerSW } from 'virtual:pwa-register'
        registerSW()
    </script>
</head>

<body class="${properties.kcBodyClass!}">
<div id="kc-page">
    <main id="kc-main">
        <div id="kc-main-wrapper">
            <div id="kc-content">
                <div id="kc-content-wrapper">

                    <#-- App-initiated actions should not see warning messages about the need to complete the action -->
                    <#-- during login.                                                                               -->
                    <#if displayMessage && message?has_content && (message.type != 'warning' || !isAppInitiatedAction??)>
                        <#if message.type = 'success'>
                            <div class="${properties.kcAlertClass!} ${properties.kcAlertSuccessClass!}">
                                <div class="pf-c-alert__icon">
                                    <#if message.type = 'success'><span class="${properties.kcFeedbackSuccessIcon!}"></span></#if>
                                </div>
                                <span class="${properties.kcAlertTitleClass!}">${kcSanitize(message.summary)?no_esc}</span>
                            </div>
                        <#elseif message.type = 'warning'>
                            <div class="${properties.kcAlertClass!} ${properties.kcAlertWarningClass!}">
                                <div class="pf-c-alert__icon">
                                    <span class="${properties.kcFeedbackWarningIcon!}"></span>
                                </div>
                                <span class="${properties.kcAlertTitleClass!}">${kcSanitize(message.summary)?no_esc}</span>
                            </div>
                        <#elseif message.type = 'error'>
                            <div class="${properties.kcAlertClass!} ${properties.kcAlertErrorClass!}">
                                <div class="pf-c-alert__icon">
                                    <span class="${properties.kcFeedbackErrorIcon!}"></span>
                                </div>
                                <span class="${properties.kcAlertTitleClass!}">${kcSanitize(message.summary)?no_esc}</span>
                            </div>
                        <#else>
                            <div class="${properties.kcAlertClass!} ${properties.kcAlertDefaultClass!}">
                                <div class="pf-c-alert__icon">
                                    <span class="${properties.kcFeedbackInfoIcon!}"></span>
                                </div>
                                <span class="${properties.kcAlertTitleClass!}">${kcSanitize(message.summary)?no_esc}</span>
                            </div>
                        </#if>
                    </#if>

                    <#nested "form">

                    <#if auth?has_content && auth.showTryAnotherWayLink() && showAnotherWayIfPresent>
                        <form id="kc-select-try-another-way-form" action="${url.loginAction}" method="post">
                            <div class="${properties.kcFormGroupClass!}">
                                <input type="hidden" name="tryAnotherWay" value="on"/>
                                <a href="#" id="try-another-way"
                                   onclick="document.forms['kc-select-try-another-way-form'].submit();return false;">${msg("doTryAnotherWay")}</a>
                            </div>
                        </form>
                    </#if>

                    <#if displayInfo>
                        <div id="kc-info" class="${properties.kcSignUpClass!}">
                            <div id="kc-info-wrapper" class="${properties.kcInfoAreaWrapperClass!}">
                                <#nested "info">
                            </div>
                        </div>
                    </#if>
                </div>
            </div>

        </div>
    </main>
</div>
</body>
</html>
</#macro>
