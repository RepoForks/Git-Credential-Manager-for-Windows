﻿/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the """"Software""""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
**/

using System.Threading.Tasks;
using Microsoft.Alm.Authentication;
using Xunit;

namespace VisualStudioTeamServices.Authentication.Test
{
    public class MsaTests : AuthenticationTests
    {
        public MsaTests()
            : base()
        { }

        [Fact]
        public async Task VstsMsaDeleteCredentialsTest()
        {
            TargetUri targetUri = DefaultTargetUri;
            MsaAuthentication msaAuthority = GetVstsMsaAuthentication(RuntimeContext.Default, "msa-delete");

            if (msaAuthority.Authority is AuthorityFake fake)
            {
                fake.CredentialsAreValid = false;
            }

            await msaAuthority.PersonalAccessTokenStore.WriteCredentials(targetUri, DefaultPersonalAccessToken);

            await msaAuthority.DeleteCredentials(targetUri);
            Assert.Null(await msaAuthority.PersonalAccessTokenStore.ReadCredentials(targetUri));

            await msaAuthority.DeleteCredentials(targetUri);
            Assert.Null(await msaAuthority.PersonalAccessTokenStore.ReadCredentials(targetUri));
        }

        [Fact]
        public async Task VstsMsaGetCredentialsTest()
        {
            TargetUri targetUri = DefaultTargetUri;
            MsaAuthentication msaAuthority = GetVstsMsaAuthentication(RuntimeContext.Default, "msa-get");

            Assert.Null(await msaAuthority.GetCredentials(targetUri));

            await msaAuthority.PersonalAccessTokenStore.WriteCredentials(targetUri, DefaultPersonalAccessToken);

            Assert.NotNull(await msaAuthority.GetCredentials(targetUri));
        }

        [Fact]
        public async Task VstsMsaInteractiveLogonTest()
        {
            TargetUri targetUri = DefaultTargetUri;
            MsaAuthentication msaAuthority = GetVstsMsaAuthentication(RuntimeContext.Default, "msa-logon");

            Assert.Null(await msaAuthority.PersonalAccessTokenStore.ReadCredentials(targetUri));

            Assert.NotNull(await msaAuthority.InteractiveLogon(targetUri, false));

            Assert.NotNull(await msaAuthority.PersonalAccessTokenStore.ReadCredentials(targetUri));
        }

        [Fact]
        public async Task VstsMsaSetCredentialsTest()
        {
            TargetUri targetUri = DefaultTargetUri;
            MsaAuthentication msaAuthority = GetVstsMsaAuthentication(RuntimeContext.Default, "msa-set");

            await msaAuthority.SetCredentials(targetUri, DefaultCredentials);

            Assert.Null(await msaAuthority.PersonalAccessTokenStore.ReadCredentials(targetUri));
        }

        [Fact]
        public async Task VstsMsaValidateCredentialsTest()
        {
            MsaAuthentication msaAuthority = GetVstsMsaAuthentication(RuntimeContext.Default, "msa-validate");
            Credential credentials = null;

            Assert.False( await msaAuthority.ValidateCredentials(DefaultTargetUri, credentials), "Credential validation unexpectedly failed.");

            credentials = DefaultCredentials;

            Assert.True(await msaAuthority.ValidateCredentials(DefaultTargetUri, credentials), "Credential validation unexpectedly failed.");
        }

        private static MsaAuthentication GetVstsMsaAuthentication(RuntimeContext context, string @namespace)
        {
            ICredentialStore tokenStore1 = new SecretCache(context, @namespace + 1, Secret.UriToIdentityUrl);
            ITokenStore tokenStore2 = new SecretCache(context, @namespace + 2, Secret.UriToIdentityUrl);
            IAuthority liveAuthority = new AuthorityFake(MsaAuthentication.QueryParameters);
            return new MsaAuthentication(context, tokenStore1, tokenStore2, liveAuthority);
        }
    }
}
