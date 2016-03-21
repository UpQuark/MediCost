/***********************************************
 *  MediCost application class
 ***********************************************/
// MediCost application class.
function MediCost() {

    this.MediCostData = {      
        // Central data model
        offices: null,              // Holds all offices retrieved from database

        // Typeahead caches (unused so far)
        specialties: null,          // Holds all medical specialties from DB
        hcpcsCodes: null,           // Holds all hcpcs codes and descriptions from DB
        cities: null,               // Holds all possible cities/towns/locations from DB
        
        // Miscellany
        activeMarker: null,         // Actively selected Google Maps marker
        procedure: "",              // Active Procedure (HCPS code)
        activeCost: null,           // Redundant with procedure?
    };

    this.MediCostPageElems = {
        mapCanvas   : null,
        header      : null,
        leftPanel   : null,
        rightPanel  : null,
        officeTable : null
    };

    // Only NEED to pass 'this', other references passed to allow for more brevity in code
    this.MediCostPageElems.mapCanvas   = new this.MapCanvas     (this, this.MediCostData, this.MediCostPageElems);
    this.MediCostPageElems.leftPanel   = new this.LeftPanel     (this, this.MediCostData, this.MediCostPageElems);
    this.MediCostPageElems.rightPanel  = new this.RightPanel    (this, this.MediCostData, this.MediCostPageElems);
    this.MediCostPageElems.officeTable = new this.OfficeTable   (this, this.MediCostData, this.MediCostPageElems);
    this.MediCostPageElems.header      = new this.Header        (this, this.MediCostData, this.MediCostPageElems);
    this.apiAgent                      = new this.ApiAgent      (this.MediCostData);
    
    this.MediCostPageElems.header.init();
}

// Empty contents of data model
MediCost.prototype.clearData = function() {
    offices = null;
};

// Reset UI to starting state
MediCost.prototype.resetUI = function () {
    this.clearData();
    this.MediCostPageElems.mapCanvas.fillFrame();
    this.MediCostPageElems.officeTable.clear();
    this.MediCostPageElems.leftPanel.clear();
    this.MediCostPageElems.rightPanel.clear();
    this.MediCostPageElems.header.reset();
};

//////// Getters ////////
MediCost.prototype.getOfficeByAddressId = function (addressId) {
    return this.MediCostData.offices[addressId];
}

MediCost.prototype.getProviderByAddressIdNpi = function (addressId, npi) {
    return this.MediCostData.offices[addressId].Providers[npi];
}



/***********************************************
 *  Header / form class
 ***********************************************/
// Constructor
MediCost.prototype.Header = function (MediCost, MediCostData, MediCostPageElems) {
    this.MediCostData = MediCostData;
    this.MediCostPageElems = MediCostPageElems;
    this.MediCost = MediCost;

    // Attach loading icon show/hide to Ajax events
    $(document).on({
        ajaxStart: function () {
            $('#loadingIcon').show();
        },
        ajaxStop: function () {
            $('#loadingIcon').hide();
        }
    });
};

// UI Init, initializes typeaheads
MediCost.prototype.Header.prototype.init = function () {
    var MediCost = this.MediCost;
    var apiAgent = new MediCost.ApiAgent();
    
    $.when(MediCost.apiAgent.getSpecialties()).done(function (response) {
        var parsedResponse = JSON.parse(response);

        arrayResponse = [];
        $.each(parsedResponse, function (key, specialty) {
            arrayResponse.push(key)
        });

        var specialties = new Bloodhound({
            datumTokenizer: Bloodhound.tokenizers.whitespace,
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            local: arrayResponse
        });

        $('#specialtyInput.typeahead').typeahead({
            hint: true,
            highlight: true,
            minLength: 1
        },
        {
            name: 'specialties',
            source: specialties
        });
    });
    $.when(MediCost.apiAgent.getHcpcsCodes()).done(function (response) {
        var parsedResponse = JSON.parse(response);

        arrayResponse = [];
        $.each(parsedResponse, function (key, code) {
            arrayResponse.push(code.Code + ': ' + code.Description);
        });

        var hcpcsCodes = new Bloodhound({
            datumTokenizer: Bloodhound.tokenizers.whitespace,
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            local: arrayResponse
        });

        $('#procedureInput.typeahead').typeahead({
            hint: true,
            highlight: true,
            minLength: 1
        },
        {
            name: 'procedure',
            source: hcpcsCodes
        });
    });
    $.when(MediCost.apiAgent.getLocations()).done(function (response) {
        var parsedResponse = JSON.parse(response);

        arrayResponse = [];
        $.each(parsedResponse, function (key, city) {
            arrayResponse.push(city.Name);
        });

        var locations = new Bloodhound({
            datumTokenizer: Bloodhound.tokenizers.whitespace,
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            local: arrayResponse
        });

        $('#addressInput.typeahead').typeahead({
            hint: true,
            highlight: true,
            minLength: 1
        },
        {
            name: 'location',
            source: locations
        })
    });

    
    /*
     * Why is this function here? The UI requires a reference to MediCostPageElems that is not set
     * as of the constructor call. This is a temporary solution that should eventually be replaced.
     */
    $('#searchbtn').click(function () {
        MediCost.MediCostPageElems.header.search();
    });

    $('#progressBar').progressbar({
        value: false
    });

    $(window).resize(function () {
        MediCost.MediCostPageElems.mapCanvas.fillFrame();
    });
};

MediCost.prototype.Header.prototype.reset = function () {
    $("#addressInput, #specialtyInput, #procedureInput").removeClass("badInput");
    $("#errorText").hide();
}

// Begins execution of office search
MediCost.prototype.Header.prototype.search = function () {
    var MediCost = this.MediCost;
    var address = null;
    var specialty = null;
    var procedureCode = null;

    // Gather user data and validate
    address = $("#addressInput").val();
    specialty = $("#specialtyInput").val();
    procedureCode = $("#procedureInput").val();

    if (isNullOrWhitespace(address) || isNullOrWhitespace(specialty)) {
        $("#errorText").html("Please enter both a city and a specialty").fadeIn();
        if (isNullOrWhitespace(address))
            $("#addressInput").addClass("badInput");

        if (isNullOrWhitespace(specialty))
            $("#specialtyInput").addClass("badInput");
    }
    else {
        MediCost.resetUI();

        // Clear any markers currently on map
        this.MediCostPageElems.mapCanvas.clearOverlays();

        // Clear panels
        this.MediCostPageElems.rightPanel.clear();
        this.MediCostPageElems.leftPanel.clear();

        // Move header to top of screen
        var fades = function () {
            return $.when($("#centerSearchText").fadeOut(), $("#headerImage").fadeOut(), $("#searchbtn").fadeOut()).done()
        }

        $.when(fades()).done(function () {
            var cssUpdate = function () {
                return $.when($("#headerImage").css({ "left": "12px", "margin-left": "12px", "margin-top": "4px", "float": "left" }),
                            $("#header").animate({
                                top: "0px",
                                marginTop: "0px",
                            }, 500)).done();
            }
            $.when(cssUpdate()).done(function () {
                $("#headerImage").fadeIn();
                $("#searchbtn").fadeIn();
                $("#search_buttonloader_wrapper").css('display', 'inline-block')
                $("#header").css({ "box-shadow": "0px 0px 5px 0px rgba(42,42,42,1.2)" })
                MediCost.MediCostPageElems.mapCanvas.initialize();
                MediCost.MediCostPageElems.mapCanvas.fillFrame();

                // TODO: Add actual address validation
                MediCost.MediCostPageElems.mapCanvas.mapOfficesFromAddress(address, specialty);
                $("#map-canvas").fadeIn();
            });
        });
    }
};



/***********************************************
 *  MapCanvas class
 ***********************************************/
// Constructor
MediCost.prototype.MapCanvas = function (MediCost, MediCostData, MediCostPageElems) {
    this.MediCostData = MediCostData;
    this.MediCostPageElems = MediCostPageElems;
    this.MediCost = MediCost;
    this.ApiAgent = MediCost.ApiAgent;

    this.Geocoder = new google.maps.Geocoder(),         // Maps geocoder for input address
    this.Map = null                                    // Google maps instance
};

MediCost.prototype.MapCanvas.prototype.initialize = function () {
    var latlng = new google.maps.LatLng(42.492786, -92.342578);
    var mapOptions = {
        zoom: 8,
        zoomControl: true,
        zoomControlOptions: {
            position: google.maps.ControlPosition.LEFT_CENTER
        },
        panControl: false,
        mapTypeControl: false,
        center: latlng
    };

    // Add google map to canvas
    this.Map = new google.maps.Map($('#map-canvas').get(0), mapOptions);
}

// TODO: Change to jquery foreach
MediCost.prototype.MapCanvas.prototype.clearOverlays = function () {
    if (this.MediCostData.offices !== null) {
        for (var i = 0; i < this.MediCostData.offices.length; i++) {
            this.MediCostData.offices[i]["marker"].setMap(null);
        }
    }
};

// Set canvas to fill screen
MediCost.prototype.MapCanvas.prototype.fillFrame = function () {
    $('#map-canvas').height($(window).height());
    $('#map-canvas').width($(window).width());
};

// Center map on inputted city and mark nearby offices on map
MediCost.prototype.MapCanvas.prototype.mapOfficesFromAddress = function(address, specialty) {
    var MediCost = this.MediCost;
    var Map = this.Map;
    var Geocoder = this.Geocoder;

    // Geocode the provided address parameter
    Geocoder.geocode({ 'address': address }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            center = results[0].geometry.location;
            Map.setCenter(center);
            Map.fitBounds(results[0].geometry.viewport);
        } else {
            alert('Could not geocode address: ' + status);
            return;
        }
    });

    // Make API calls to backend, wait for ajax calls to finish
    $.when(this.MediCost.apiAgent.getOffices(address, specialty)).done(function(officeResponse) {
        MediCost.MediCostData.offices = JSON.parse(officeResponse);
            
        if (MediCost.MediCostData.offices.length == 0) {
            MediCost.resetUI();
            return;
        }

        $.each(MediCost.MediCostData.offices, function(a, b) {
            MediCost.MediCostPageElems.mapCanvas.placeOffice(b);
        });
        MediCost.MediCostPageElems.officeTable.draw();
    });
};

// Places office markers on map
MediCost.prototype.MapCanvas.prototype.placeOffice = function(office) {
    // location of office
    if (office.Latitude != null && office.Longitude != null) {
        var oloc = new google.maps.LatLng(office.Latitude, office.Longitude);
        // marker to signal where office is located
        var marker = this.createMarker(oloc, office);
        office.marker = marker;
    }
};

MediCost.prototype.MapCanvas.prototype.createMarker = function(position, office) {
    var MediCost = this.MediCost;
    var Map = this.Map;

    var marker = new google.maps.Marker({
        position: position,
        map: this.Map,
        animation: google.maps.Animation.DROP,
        title: office.AddressID,
        office: office
    });

    office.marker = marker;
    
    /**
   * Marker that was clicked is set as only bouncing marker
   * @param {Google Marker} marker The marker that will be sole bouncer
   */
    
    var clickListener = function() {
        if (MediCost.MediCostData.activeMarker != this) {
            MediCost.MediCostPageElems.mapCanvas.setActiveMarker(this);
            var office = this.office;
            MediCost.MediCostPageElems.rightPanel.draw(office);
            var latlng = new google.maps.LatLng(office.Latitude, office.Longitude);
            Map.panTo(latlng);
        }
    };
    google.maps.event.addListener(marker, 'click', clickListener);
    return marker;
};

MediCost.prototype.MapCanvas.prototype.setActiveMarker = function (marker) {
    var MediCost = this.MediCost;
    // change marker animation
    if (MediCost.MediCostData.activeMarker) {
        MediCost.MediCostData.activeMarker.setAnimation(null);
    }
    marker.setAnimation(google.maps.Animation.BOUNCE);
    MediCost.MediCostData.activeMarker = marker;
}



/***********************************************
 *  ApiAgent class
 ***********************************************/
// Constructor
MediCost.prototype.ApiAgent = function (MediCostData) {
    this.MediCostData = MediCostData;
};

MediCost.prototype.ApiAgent.prototype.getOffices = function (address, specialty) {
    var MediCostData = this.MediCostData;
    return $.ajax({
        url: '/MediCostAPI/api/MediCost/',
        type: "GET",
        crossDomain: "true",
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        data: {
            specialty: encodeURIComponent(specialty),
            city: encodeURIComponent(address)
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Office lookup failed.");
        },
        dataType: "JSON"
    });
};

MediCost.prototype.ApiAgent.prototype.getSpecialties = function () {
    var MediCostData = this.MediCostData;
    return $.ajax({
        url: '/MediCostAPI/api/Specialties/',
        type: "GET",
        crossDomain: "true",
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Specialties lookup failed.");
        },
        dataType: "JSON"
    });
};

MediCost.prototype.ApiAgent.prototype.getHcpcsCodes = function () {
    return $.ajax({
        url: '/MediCostAPI/api/HcpcsCode/',
        type: "GET",
        crossDomain: "true",
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Code lookup failed.");
        },
        dataType: "JSON"
    });
};

MediCost.prototype.ApiAgent.prototype.getLocations = function () {
    return $.ajax({
        url: '/MediCostAPI/api/Locations/',
        type: "GET",
        crossDomain: "true",
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Location lookup failed.");
        },
        dataType: "JSON"
    });
}

/***********************************************
 *  LeftPanel class
 ***********************************************/
// Constructor
MediCost.prototype.LeftPanel = function (MediCost, MediCostData, MediCostPageElems) {
    this.MediCostData = MediCostData;
    this.MediCostPageElems = MediCostPageElems;
    this.MediCost = MediCost;

    this.currentProvider = null;

    $("#left-panel").resizable({
        handles: "e, n, ne"
    });

    //$("#left-panel").draggable();
};

MediCost.prototype.LeftPanel.prototype.clear = function () {
    this.currentProvider = null
    $("#cost-table #table_content").empty();
    $("#left-panel").fadeOut();
}

MediCost.prototype.LeftPanel.prototype.draw = function (provider) {
    var MediCost = this.MediCost;
    this.currentProvider = provider;

    $("#leftPanelProviderName").html(provider.FirstName + " " + provider.LastName);

    $.each(provider.Costs, function (a, cost) {
        var toAppend = '<tr class="leftPanelCostRow panel-link"><td>'
                       + cost.hcpcsCode
                       + "</td><td class=panel-link>"
                       + cost.HcpcsDescription
                       + "</td><td class=panel-link>$"
                       + parseFloat(cost.AvgMedicareAllowedAmount).toFixed(2)
                       + "</td><td class=panel-link>$"
                       + parseFloat(cost.AvgSubmittedChargeAmount).toFixed(2)
                       + "</td><td class=panel-link>$"
                       + parseFloat(cost.AvgMedicarePaymentAmount).toFixed(2)
                       + "</td><td class=panel-link>$"
                       + parseFloat((cost.AvgMedicareAllowedAmount) - parseFloat(cost.AvgMedicarePaymentAmount)).toFixed(2)
                       + '</td></tr>'
                       

        //LineServiceCount = r[5].ToString(), Number of services provided; note that the metrics used to count the number provided can vary from service to service.
        //BenefitsUniqueCount = r[6].ToString(), – Number of distinct Medicare beneficiaries receiving the service
        //BenefitsDayServiceCount = r[7].ToString(), Number of distinct Medicare beneficiary/per day services. Since a given
        //beneficiary may receive multiple services of the same type (e.g., single vs. multiple cardiac stents) on a
        //single day, this metric removes double-counting from the line service count to identify whether a unique
        //service occurred.

        //AvgMedicareAllowedAmount = r[8].ToString(),
        //AvgMedicareAllowedAmountStDev = r[9].ToString(),
        //AvgSubmittedChargeAmount = r[10].ToString(),
        //AvgSubmittedChargeAmountStDev = r[11].ToString(),
        //AvgMedicarePaymentAmount = r[12].ToString(),
        //AvgMedicarePaymentAmountStDev = r[13].ToString(),


        $("#cost-table #table_content").append(toAppend);
    });

    $("#cost-table").tablesorter();
    $("#left-panel").fadeIn();
}



/***********************************************
 *  RightPanel class
 ***********************************************/
// Constructor
MediCost.prototype.RightPanel = function (MediCost, MediCostData, MediCostPageElems) {
    this.MediCostData = MediCostData;
    this.MediCostPageElems = MediCostPageElems;
    this.MediCost = MediCost;

    this.currentOffice = null;

    $("#right-panel").resizable({
            handles: "w, s, sw"
    });

    //$("#right-panel").draggable();
};

MediCost.prototype.RightPanel.prototype.clear = function () {
    $('#right-panel').fadeOut();
    $("#providersListTable").empty();
    this.currentOffice = null;
};

// creates sidebar with office's information when clicking the first cell on the table
MediCost.prototype.RightPanel.prototype.draw = function (office) {
    var MediCost = this.MediCost;
    this.currentOffice = office;

    $("#right-panel #address").html(office.Street1 + ' ' + office.Street2);
    $("#right-panel #city").html(office.City);
    $("#right-panel #state").html(office.State);
    $("#right-panel #zip").html(office.Zip);
    $("#right-panel #addressId").html(office.AddressID);

    $("#providersListTable tbody").html("");
    $.each(office.Providers, function (a, provider) {
        var middleInitial = typeof provider.MiddleIninitial === "undefined" ? "" : provider.MiddleIninitial + " ";
        var toAppend = '<tr class="rightPanelProviderCell" data-npi="' + provider.Npi + '" data-addressid = "' + provider.AddressID + '"><td class=panel-link>'
                       + provider.FirstName
                       + " "
                       + middleInitial
                       + provider.LastName
                       + '</td></tr>'

        $("#providersListTable").append(toAppend);
    });

    $(".rightPanelProviderCell").click(function () {
        $("#cost-table #table_content").empty();
        var npi = $(this).data('npi');
        var addressId = $(this).data('addressid');
        var provider = MediCost.getProviderByAddressIdNpi(addressId, npi);
        MediCost.MediCostPageElems.leftPanel.draw(provider);
    })
    //$("#right-panel").css("min-height", $("#right-panel").css('height'));
    $("#right-panel").fadeIn();
};



/***********************************************
 *  OfficeTable  class
 ***********************************************/
// Constructor
MediCost.prototype.OfficeTable = function (MediCost, MediCostData, MediCostPageElems) {
    this.MediCostData = MediCostData;
    this.MediCostPageElems = MediCostPageElems;
    this.MediCost = MediCost;

    $("#offices-panel").resizable({
        handles: "w, n, nw",
    });

    //$("#offices-panel").draggable();
};

MediCost.prototype.OfficeTable.prototype.clear = function () {
    $('#offices-panel').fadeOut();
    $('#office-table #table_content').empty();
};

MediCost.prototype.OfficeTable.prototype.draw = function () {
    var MediCost = this.MediCost;

    $.each(MediCost.MediCostData.offices, function (a, office) {
        var toAppend = '<tr class="officeTableRow panel-link" data-addressid="' + office.AddressID + '"><td>'
                       + office.Street1 //+ ' ' + office.Street2
                       + "</td><td>"
                       + office.City
                       + "</td><td>"
                       + office.State
                       + "</td><td>"
                       + office.Zip.substring(0, 5)
                       + '</td></tr>'

        $("#office-table").append(toAppend);
    });

    $(".officeTableRow").click(function () {
        var addressId = $(this).data('addressid');
        var office = MediCost.getOfficeByAddressId(addressId);
        MediCost.MediCostPageElems.rightPanel.draw(office);
        MediCost.MediCostPageElems.mapCanvas.setActiveMarker(office.marker);
    })

    $("#office-table").tablesorter();
    $('#offices-panel').fadeIn();
}



/***********************************************
 *  Helper functions
 ***********************************************/
function isNullOrWhitespace(input) {

    if (typeof input === 'undefined' || input == null) return true;

    return input.replace(/\s/g, '').length < 1;
}
