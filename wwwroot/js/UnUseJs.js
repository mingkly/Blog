var lastSelect = new Range();
var lastElement;
function SetTextProperty(property, cssValue) {
    var select = GetSelectionRef();
    if (select == null) {
        $(p).css(property, cssValue);
        return;
    }
    var s;
    /*
    if (select.startContainer == select.endContainer && select.startOffset == select.endOffset) {
        $(p).css(property, cssValue);
        return;
    }
    */
    if (select == lastSelect && property == lastElement.getAttribute("idz")) {
        s = lastElement;
    }
    else {
        s = document.createElement("span");
        s.setAttribute("id", property);
        s.append(select.extractContents());
        select.insertNode(s);
    }
    $(s).css(property, cssValue);
    SetInnerProperty(s, property, cssValue);
    RemoveEmpty();
    RemoveSame(p);
    lastSelect = select.cloneRange();
    lastElement = s;
}
function RemoveEmpty() {
    $.each($(p).find("span"), function (key, value) {
        if ($(value).html() == "") {
            $(value).remove();
        }
    })
}
function RemoveSblings(span) {
    var property = $(span).attr("id");
    if (span.nextSibling == null) {
        return;
    }
    var s = span.nextSibling.textContent.replace(/[\n\s]*/g, "");
    if (s == "") {
        span.nextSibling.textContent = s;
        if ($(span).next("span").attr("id") == property &&
            $(span).next("span").css(property) == $(span).css(property)) {
            $(span).append($(span).next("span").html());
            $(span).next("span").remove();
            RemoveSblings(span);
        }
    }

}
function RemoveSame(span) {
    $.each($(span).children("span"), function (key, value) {
        RemoveSame(value);
        if ($(span).attr("id") == $(value).attr("id") && $(span).html() == value) {
            var s = $(span).html();
            $(span).before(s);
            $(span).remove();
        }
        RemoveSblings(value);
    })
}
function SetNormal() {
    SetTextProperty('font-weight', 'normal');
    SetTextProperty('font-style', 'normal');
}

function SetInnerProperty(span, property, cssValue) {

    var spans = $(span).children("span");
    $.each(spans, function (key, value) {
        SetInnerProperty(value, property, cssValue);
        console.log($(value).attr("id"));
        if ($(value).attr("id") == property) {
            var innerContent = $(value).html();
            $(value).before(innerContent);
            $(value).remove();
        }
        else {
            $(value).css(property, cssValue);
        }
    })
}
function SetFontSize(size) {
    SetTextProperty("font-size", size);
    $("#fontSizeForShow").html(size);
}
function GetSelection() {
    if (isEditing && document.getSelection().rangeCount > 0) {
        var x = document.getSelection().getRangeAt(0).cloneRange();
        return x;
    }
    return null;
}
function GetSelectionRef() {
    if (document.getSelection().rangeCount > 0) {
        var x = document.getSelection().getRangeAt(0);
        var p = x.startContainer;
        if (IsParentP(p) && IsParentP(x.endContainer)) {
            return x;
        }
    }
    return null;
}