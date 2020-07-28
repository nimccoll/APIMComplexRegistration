//===============================================================================
// Microsoft FastTrack for Azure
// APIM Complex Registration POC
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using System;
using System.Collections.Generic;

namespace ScanSource.Services.Models
{
    public class Identity
    {
        public string provider { get; set; }
        public string id { get; set; }

    }

    public class UserProperties
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string state { get; set; }
        public DateTime registrationDate { get; set; }
        public object note { get; set; }
        public List<Identity> identities { get; set; }

    }

    public class User
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public UserProperties properties { get; set; }

    }

    public class Users
    {
        public List<User> value { get; set; }
        public int count { get; set; }

    }

}
