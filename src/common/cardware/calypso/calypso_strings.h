#ifdef EXPAND_STRINGS
  #undef ADD_STRING
  #define ADD_STRING(a, b) const char a[] = b;
#else
  #define ADD_STRING(a, b) extern const char a[]
#endif

ADD_STRING(strAtr, "ATR");
ADD_STRING(strUid, "UID");
ADD_STRING(strStatus, "Status");
ADD_STRING(strRevision, "Revision");

ADD_STRING(strSam, "SAM");

ADD_STRING(strRead, "Read");
ADD_STRING(strParse, "Parse");

ADD_STRING(strEnvironment, "Environment");
ADD_STRING(strNetwork, "Network");
ADD_STRING(strEndDate, "EndDate");
ADD_STRING(strIssuer, "Issuer");
ADD_STRING(strContract, "Contract");
ADD_STRING(strProvider, "Provider");
ADD_STRING(strTariff, "Tariff");
ADD_STRING(strZones, "Zones");
ADD_STRING(strStart, "Start");
ADD_STRING(strEnd, "End");
ADD_STRING(strCommit, "Commit");
ADD_STRING(strTransaction, "Transaction");
ADD_STRING(strAuthenticator, "Authenticator");

#if (!defined(CALYPSO_MINIMAL_STRINGS))

ADD_STRING(strCard, "Card");
ADD_STRING(strData, "Data");

ADD_STRING(strCalypso, "Calypso");

ADD_STRING(strSw, "SW");
ADD_STRING(strDf, "DF");
ADD_STRING(strFci, "FCI");

ADD_STRING(strFileInfo, "FileInfo");
ADD_STRING(strCardInfo, "CardInfo");

ADD_STRING(strHistBytes, "HistBytes");
ADD_STRING(strIssuerTag, "IssuerTag");

ADD_STRING(strFormat, "Format");
ADD_STRING(strVersion, "Version");
ADD_STRING(strType, "Type");
ADD_STRING(strSubType, "SubType");
ADD_STRING(strMaxMods, "MaxMods");
ADD_STRING(strPlatform, "Platform");
ADD_STRING(strSoftIssuer, "SoftIssuer");
ADD_STRING(strSoftVer, "SoftVer");
ADD_STRING(strSoftRev, "SoftRev");

ADD_STRING(strWithPin, "WithPin");
ADD_STRING(strWithStoredValue, "WithStoredValue");
ADD_STRING(strNeedRatificationFrame, "NeedRatificationFrame");

ADD_STRING(strRecord, "Record");

ADD_STRING(strHolder, "Holder");
ADD_STRING(strEnvHolder, "EnvHolder");
ADD_STRING(strContracts, "Contracts");
ADD_STRING(strTransportLog, "TransportLog");


ADD_STRING(strNumber, "Number");
ADD_STRING(strName, "Name");
ADD_STRING(strFirst, "First");
ADD_STRING(strLast, "Last");
ADD_STRING(strBirth, "Birth");
ADD_STRING(strPlace, "Place");
ADD_STRING(strCountry, "Country");
ADD_STRING(strSelect, "Select");
ADD_STRING(strProduct, "Product");

ADD_STRING(strLivePlace, "LivePlace");
ADD_STRING(strWorkPlace, "WorkPlace");
ADD_STRING(strStudyPlace, "StudyPlace");

ADD_STRING(strDataCardStatus, "DataCardStatus");

ADD_STRING(strPayMethod, "PayMethod");
ADD_STRING(strPayPointer, "PayPointer");
ADD_STRING(strRemotePay, "RemotePay");


ADD_STRING(strTimeCode, "TimeCode");

ADD_STRING(strLocation, "Location");
ADD_STRING(strLocationGate, "LocGate");
ADD_STRING(strLocationType, "LocType");
ADD_STRING(strLocationRef, "LocRef");

ADD_STRING(strLoyalty, "Loyalty");
ADD_STRING(strEmployee, "Employee");
ADD_STRING(strSimul, "Simul");

ADD_STRING(strTrip, "Trip");
ADD_STRING(strDirection, "Direction");


ADD_STRING(strRaw, "Raw");
ADD_STRING(strBitmap, "Bitmap");


ADD_STRING(strNetworkCountry, "NetworkCountry");
ADD_STRING(strNetworkIndex, "NetworkIndex");
ADD_STRING(strServices, "Services");

ADD_STRING(strStartDate, "StartDate");
ADD_STRING(strStartTime, "StartTime");
ADD_STRING(strEndTime, "EndTime");
ADD_STRING(strDay, "Day");
ADD_STRING(strCode, "Code");
ADD_STRING(str_Authenticator_C, "_Authenticator_C");
ADD_STRING(strProfile, "Profile");

ADD_STRING(strClass, "Class");
ADD_STRING(strTotal, "Total");
ADD_STRING(strClassAllowed, "ClassAllowed");
ADD_STRING(strPriceAmount, "PriceAmount");
ADD_STRING(strPriceUnit, "PriceUnit");
ADD_STRING(strCustomer, "Customer");
ADD_STRING(strRestrict, "Restrict");
ADD_STRING(strValidity, "Validity");
ADD_STRING(strDuration, "Duration");
ADD_STRING(strLimitDate, "LimitDate");
ADD_STRING(strZoneList, "ZoneList");
ADD_STRING(strValidJourneys, "ValidJourneys");
ADD_STRING(strPeriodJourneys, "PeriodJourneys");
ADD_STRING(strJourney, "Journey");
ADD_STRING(strOrigin, "Origin");
ADD_STRING(strDestination, "Destination");
ADD_STRING(strRouteNumbers, "RouteNumbers");
ADD_STRING(strRouteVariants, "RouteVariants");
ADD_STRING(strRun, "Run");
ADD_STRING(strVia, "Via");
ADD_STRING(strDistance, "Distance");
ADD_STRING(strInterchanges, "Interchanges");
ADD_STRING(strSale, "Sale");
ADD_STRING(strDate, "Date");
ADD_STRING(strTime, "Time");
ADD_STRING(strCompany, "Company");
ADD_STRING(strDevice, "Device");

ADD_STRING(strResult, "Result");
ADD_STRING(strDisplayData, "DisplayData");
ADD_STRING(strServiceProvider, "ServiceProvider");
ADD_STRING(strNotOKCounter, "NotOKCounter");
ADD_STRING(strPassenger, "Passenger");
ADD_STRING(strVehicle, "Vehicle");
ADD_STRING(strVehicleClass, "VehicleClass");
ADD_STRING(strRouteNumber, "RouteNumber");
ADD_STRING(strRouteVariant, "RouteVariant");
ADD_STRING(strJourneyRun, "JourneyRun");
ADD_STRING(strJourneyInterchanges, "JourneyInterchanges");
ADD_STRING(strJourneyDistance, "JourneyDistance");
ADD_STRING(strTotalJourneys, "TotalJourneys");

ADD_STRING(strContractPointer, "ContractPointer");
ADD_STRING(strContractListTariff, "ListTariff");
ADD_STRING(strContractListPointer, "ContractListPointer");
ADD_STRING(strDiagnosticCounter, "DiagnosticCounter");

#endif
