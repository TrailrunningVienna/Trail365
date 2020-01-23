// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function initDataTable() {
  $('#datatable').DataTable({
    "language": {
      "sEmptyTable": "Keine Daten in der Tabelle vorhanden",
      "sInfo": "_START_ bis _END_ von _TOTAL_ Einträgen",
      "sInfoEmpty": "Keine Daten vorhanden",
      "sInfoFiltered": "(gefiltert von _MAX_ Einträgen)",
      "sInfoPostFix": "",
      "sInfoThousands": ".",
      "sLengthMenu": "_MENU_ Einträge anzeigen",
      "sLoadingRecords": "Wird geladen ..",
      "sProcessing": "Bitte warten ..",
      "sSearch": "Suchen",
      "sZeroRecords": "Keine Einträge vorhanden",
      "oPaginate": {
        "sFirst": "Erste",
        "sPrevious": "Zurück",
        "sNext": "Nächste",
        "sLast": "Letzte"
      },
      "oAria": {
        "sSortAscending": ": aktivieren, um Spalte aufsteigend zu sortieren",
        "sSortDescending": ": aktivieren, um Spalte absteigend zu sortieren"
      },
      "select": {
        "rows": {
          "_": "%d Zeilen ausgewählt",
          "0": "",
          "1": "1 Zeile ausgewählt"
        }
      },
      "buttons": {
        "print": "Drucken",
        "colvis": "Spalten",
        "copy": "Kopieren",
        "copyTitle": "In Zwischenablage kopieren",
        "copyKeys":
          "Taste <i>ctrl</i> oder <i>\u2318</i> + <i>C</i> um Tabelle<br>in Zwischenspeicher zu kopieren.<br><br>Um abzubrechen die Nachricht anklicken oder Escape drücken.",
        "copySuccess": {
          "_": "%d Zeilen kopiert",
          "1": "1 Zeile kopiert"
        },
        "pageLength": {
          "-1": "Zeige alle Zeilen",
          "_": "Zeige %d Zeilen"
        }
      }
    }
  });
}

function goBack() {
  window.history.back();
}

function changeFavorite(myDivId, url) {
  var xmlhttp;
  if (window.XMLHttpRequest) {
    xmlhttp = new XMLHttpRequest();
  }
  else {
    xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
  }

  xmlhttp.onreadystatechange = function () {
    if (xmlhttp.readyState === XMLHttpRequest.DONE) {
      if (xmlhttp.status === 200) {
        document.getElementById(myDivId).outerHTML = xmlhttp.responseText;
        var allScripts = document.getElementById(myDivId).getElementsByTagName('script');
        for (var n = 0; n < allScripts.length; n++) {
          eval(allScripts[n].innerHTML);
        }
      }
    }
  }

  xmlhttp.open("GET", url, true);
  xmlhttp.send();
}

function removeImage(myDivId, url) {
  var xmlhttp;
  if (window.XMLHttpRequest) {
    xmlhttp = new XMLHttpRequest();
  }
  else {
    xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
  }

  xmlhttp.onreadystatechange = function () {
    if (xmlhttp.readyState === XMLHttpRequest.DONE) {
      if (xmlhttp.status === 200) {
        document.getElementById(myDivId).innerHTML = xmlhttp.responseText;
        var allScripts = document.getElementById(myDivId).getElementsByTagName('script');
        for (var n = 0; n < allScripts.length; n++) {
          eval(allScripts[n].innerHTML);
        }
      }
    }
  }

  xmlhttp.open("GET", url, true);
  xmlhttp.send();
}

function postMessage(myDivId, url, message, dealId, fromUserId, toUserId) {
  var xmlhttp;
  if (window.XMLHttpRequest) {
    xmlhttp = new XMLHttpRequest();
  }
  else {
    xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
  }

  xmlhttp.onreadystatechange = function () {
    if (xmlhttp.readyState === XMLHttpRequest.DONE) {
      if (xmlhttp.status === 200) {
        document.getElementById(myDivId).innerHTML = xmlhttp.responseText;
        var allScripts = document.getElementById(myDivId).getElementsByTagName('script');
        for (var n = 0; n < allScripts.length; n++) {
          eval(allScripts[n].innerHTML);
        }
      }
    }
  }

  var data = new FormData();
  data.append('Message', message);
  data.append('DealID', dealId);
  data.append('FromUserID', fromUserId);
  data.append('ToUserID', toUserId);

  xmlhttp.open("POST", url, true);
  xmlhttp.send(data);
}

function uploadFile(myDivId, url, file) {
  var xmlhttp;
  if (window.XMLHttpRequest) {
    xmlhttp = new XMLHttpRequest();
  }
  else {
    xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
  }

  xmlhttp.onreadystatechange = function () {
    if (xmlhttp.readyState === XMLHttpRequest.DONE) {
      if (xmlhttp.status === 200) {
        document.getElementById(myDivId).innerHTML = xmlhttp.responseText;
        var allScripts = document.getElementById(myDivId).getElementsByTagName('script');
        for (var n = 0; n < allScripts.length; n++) {
          eval(allScripts[n].innerHTML);
        }
      }
    }
  }

  var data = new FormData();
  data.append('File', file);

  xmlhttp.open("POST", url, true);
  xmlhttp.send(data);
}

function uploadFiles(myDivId, url, newFiles, oldFiles) {
  var xmlhttp;
  if (window.XMLHttpRequest) {
    xmlhttp = new XMLHttpRequest();
  }
  else {
    xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
  }

  xmlhttp.onreadystatechange = function () {
    if (xmlhttp.readyState === XMLHttpRequest.DONE) {
      if (xmlhttp.status === 200) {
        document.getElementById(myDivId).innerHTML = xmlhttp.responseText;
        var allScripts = document.getElementById(myDivId).getElementsByTagName('script');
        for (var n = 0; n < allScripts.length; n++) {
          eval(allScripts[n].innerHTML);
        }
      }
    }
  }

  var data = new FormData();
  data.append('existingFiles', oldFiles);
  for (var i = 0; i < newFiles.length; i++) {
    data.append(newFiles[i].name, newFiles[i]);
  }

  xmlhttp.open("POST", url, true);
  xmlhttp.send(data);
}

function toggleFilters() {
  var filterContainer = document.getElementById('filter-wrapper');
  if (filterContainer && filterContainer.classList.contains('visible')) {
    filterContainer.classList.remove('visible');
  } else {
    filterContainer.classList.add('visible');
  }

  var sortOrderContainer = document.getElementById('sort-wrapper');
  if (sortOrderContainer && sortOrderContainer.classList.contains('visible')) {
    sortOrderContainer.classList.remove('visible');
  }
}

function toggleSortOrders() {
  var sortOrderContainer = document.getElementById('sort-wrapper');
  if (sortOrderContainer && sortOrderContainer.classList.contains('visible')) {
    sortOrderContainer.classList.remove('visible');
  } else {
    sortOrderContainer.classList.add('visible');
  }

  var filterContainer = document.getElementById('filter-wrapper');
  if (filterContainer && filterContainer.classList.contains('visible')) {
    filterContainer.classList.remove('visible');
  }
}

// ReSharper disable once InconsistentNaming
function displayAttributes(categoryID) {
  var allAttributeContainer = document.querySelectorAll('.attributes'), i = 0, l = allAttributeContainer.length;
  var allAttributeInputs = document.querySelectorAll('.attrVal'), j = 0, m = allAttributeInputs.length;

  for (i; i < l; i++) {
    allAttributeContainer[i].classList.add('d-none');
  }
  for (j; j < m; j++) {
    allAttributeInputs[j].setAttribute("disabled", "disabled");
  }

  if (categoryID !== '') {
    var attributeContainer = document.getElementById(categoryID);
    if (attributeContainer) {
      attributeContainer.classList.remove('d-none');
      var attributeInputs = attributeContainer.querySelectorAll('.attrVal'), k = 0, n = attributeInputs.length;
      for (k; k < n; k++) {
        attributeInputs[k].removeAttribute("disabled");
      }
    }
  }
}

// ReSharper disable once InconsistentNaming
function initSubcategory(dropDownID, selectedValue, reset) {
  var dropDown = document.getElementById(dropDownID);
  var hidden = document.getElementById('KategorieID');
  if (!dropDown) {
    hidden.value = selectedValue;
    return;
  } else {
    hidden.value = '';
  }

  while (dropDown.length > 0) {
    dropDown.remove(0);
  }

  var source = document.getElementById('cat_' + selectedValue);
  if (source) {
    dropDown.innerHTML = source.innerHTML;
    dropDown.hidden = false;
  } else {
    dropDown.hidden = true;
    hidden.value = selectedValue;
  }

  if (reset === true && dropDown[0]) {
    dropDown[0].selected = true;
  }
}

// ReSharper disable once InconsistentNaming
function toggleField(attributeID) {
  if (attributeID !== '') {
    var attributeContainer = document.getElementById(attributeID);
    var attributeFieldContainer = document.getElementById(attributeID + '-field');

    if (attributeContainer.classList.contains('open')) {
      attributeContainer.classList.remove('open');
      attributeFieldContainer.classList.add('d-none');
    } else {
      attributeContainer.classList.add('open');
      attributeFieldContainer.classList.remove('d-none');
    }
  }
}

// ReSharper disable once InconsistentNaming
function toggleKontaktdaten(contactID) {
  var divs = document.getElementsByClassName("kontakt");
  for(var i = 0; i < divs.length; i++) {
    if (contactID === '00000000-0000-0000-0000-000000000000') {
      divs[i].classList.remove('d-none');
    } else {
      divs[i].classList.add('d-none');
    }
  }
}
