// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$.format = function (source, params) {
    if (arguments.length == 1)
        return function () {
            var args = $.makeArray(arguments);
            args.unshift(source);
            return $.format.apply(this, args);
        };
    if (arguments.length > 2 && params.constructor != Array) {
        params = $.makeArray(arguments).slice(1);
    }
    if (params.constructor != Array) {
        params = [params];
    }
    $.each(params, function (i, n) {
        console.log(i);
        source = source.replace(new RegExp("\\{" + i + "\\}", "g"), n);
    });
    
    return source;
};
async function OnSearch(input, word) {
    console.log(word);
    $("#searchRecord").html("");

    var res = await fetch("api/articles/GetSearchRecord/" + word, {
    })
    if (res.ok) {
        var json = await res.json();
        if (json.id == 1) {
            $("#searchRecord").css("display", "");           
            for (r in json.records) {
                $("#searchRecord").append("<li class='nav-item'><a class='nav-link' style='width:100%' href='articles/searchresults?word=" + json.records[r].word + "&usingKeyword=true'>" + json.records[r].word + "</a></li>");
            }
        }
    }
}
function LeaveSeach() {
    setInterval(function () {
        $("#searchRecord").css("display", "none");
        clearInterval();
    }, 200);

}
async function Vote(up, id, bt) {
    try {
        
        var response = await fetch('api/Articles/VoteArticle', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'content-type': 'application/json',
                'accept': 'application/json'
            },
            body: JSON.stringify({
                Action: up,
                Id: id
            })
        });
        if (response.ok) {
            var json = await response.json();
            if (json.id == "suc") {
                var vote;
                if (up > 0) {
                    vote = 'VoteUpActive';
                }
                else if (up < 0) {
                    vote = 'VoteDownActive';
                }
                var cl = $(bt).attr("class")
                $(bt).attr("class", cl + vote);

                var n = $(bt).next().html();
                var x = parseInt(n);
                $(bt).next().html(++x);
            }
            else {
                alert(json.id + ":  " + json.msg);
            }

        }
    }
    catch (ex) {
        console.error('error:', ex);
    }
}

async function VoteC(up, id, bt) {
    try {
        var response = await fetch('api/Articles/VoteComment', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'content-type': 'application/json',
                'accept': 'application/json'
            },
            body: JSON.stringify({
                Action: up,
                Id: id,
            })
        });
        if (response.ok) {
            var json = await response.json();
            if (json.id == "suc") {
                var vote;
                var bt1;
                var bt2;
                if (up == 1) {
                    bt1 = bt;
                    bt2 = $(bt).parent().next().children().get(0);
                }
                else if (up == -1) {
                    bt2 = bt;
                    bt1 = $(bt).parent().prev().children().get(0);
                }
                up = json.msg;
                if (up > 0) {
                    $(bt1).attr("class", "VoteUp VoteUp-sm  VoteUpActive");
                    $(bt2).attr("class", "VoteDown VoteDown-sm ");
                }
                else if (up < 0) {
                    $(bt1).attr("class", "VoteUp VoteUp-sm ");
                    $(bt2).attr("class", "VoteDown VoteDown-sm  VoteDownActive");
                }
                else {
                    $(bt1).attr("class", "VoteUp VoteUp-sm ");
                    $(bt2).attr("class", "VoteDown  VoteDown-sm ");
                }
                $(bt1).next().html(json.voteUpCount);
                $(bt2).next().html(json.voteDownCount);
            }
            else {
                alert(json.id + ":  " + json.msg);
            }

        }
    }
    catch (ex) {
        console.error('error:', ex);
    }
}
var prevInput;
var prevButton;
function Reply(number, button) {
    var form = $(button).next().get(0);
    if (prevInput != null) {
        prevInput.style.display = "none";
        prevButton.style.display = "block";
    }
    button.style.display = "none";
    prevButton = button;
    form.style.display = "block";
    prevInput = form;
}
async function Favourite(userName,userAvatar,text) {
    try {
        var response = await fetch('api/Articles/FavouriteArticle', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'content-type': 'application/json',
                'accept': 'application/json'
            },
            body: JSON.stringify({
                Id: id
            })
        })
        if (response.ok) {
            var b = document.getElementById('Favourite');
            var favourited = await response.json()
            if (favourited.id == "1") {
                $(b).attr("class", "Favourite FavouriteActive");
            }
            else if (favourited.id == "0") {
                $(b).attr("class", "Favourite");
            }

        }
    }
    catch (ex) {
        console.error('error', ex);
    }
}

async function LoadMoreComment() {
    var response = await fetch('api/Articles/LoadMoreArticleComment', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
            'content-type': 'application/json',
            'accept': 'application/json'
        },
        body: JSON.stringify({
            ArticleId: articleId,
            CommentId: commentId,
        })
    });
    if (response.ok) {
        var comments = await response.json();
        $.each(comments, function (key, value) {

            var c = GetCommentElement(value);
            $("#commentList").append($(c));
            commentId = value.id;
        })
        if (comments.length < 3) {
            $("#loadmoreBtn").css("display", "none");
        }
    }
}
async function LoadMoreInnerComment(id,property,list,btn) {
    var response = await fetch('api/Articles/LoadMoreInnerComment', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
            'content-type': 'application/json',
            'accept': 'application/json'
        },
        body: JSON.stringify({
            InnerCommentId: property,
            CommentId: id,
        })
    });
    if (response.ok) {
        console.log(list);
        var comments = await response.json();
        $.each(comments, function (key, value2) {
            var upactive = "";
            var downactive = "";
            if (value2.currentUserAction == 1) {
                upactive = "VoteUpActive";
            }
            else if (value2.currentUserAction == -1) {
                downactive = "VoteDownActive";
            }
            var $innerTemp = $("#innerComments").clone();
            $innerTemp.css("display", "");
            $innerTemp.find("div[id=userAndText2]").html($.format($("#userAndText2").html(), value2.userName, value2.replyToCommentUserName, value2.text));
            $innerTemp.find("div[id=timeAndVote]").html($.format($("#timeAndVote").html(), value2.time, upactive, value2.id, value2.voteUpCount, downactive, value2.voteDownCount));
            $(list).append($innerTemp);
            $(btn).attr("onclick", $.format("LoadMoreInnerComment({0},{1},this.parentElement,this)", id, value2.id));
        })
        $(list).append($(btn));
        console.log(comments.length);
        if (comments.length<3) {
            $(btn).css("display", "none");
        }
    }
}

async function LoadMoreArticles(listId,btn) {
    var response = await fetch('api/Articles/LoadMoreArticleViews/' + articleViewId, {
        method: 'GET',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
            'content-type': 'application/json',
            'accept': 'application/json'
        },
    });
    if (response.ok) {
        var comments = await response.json();
        $.each(comments, function (key, value) {

            var c = GetArticleElement(value);
            $(listId).append($(c));
            articleViewId = value.id;
        })
        if (comments.length < 3) {
            $(btn).css("display", "none");
        }
    }
}
function GetCommentElement(value) {
    var upactive = "";
    var downactive = "";
    if (value.currentUserAction == 1) {
        upactive = "VoteUpActive";
    }
    else if (value.currentUserAction == -1) {
        downactive = "VoteDownActive";
    }
    
    var $temp = $("#commentTemp").clone();
    var $outterTemp = $($temp.find("div[id=outterTemp]").get(0));
    $outterTemp.find("div[id=userAndText]").html($.format($("#userAndText").html(), value.userName, value.userAvatar, value.text));
    $outterTemp.find("div[id=timeAndVote]").html($.format($("#timeAndVote").html(), value.time, upactive, value.id, value.voteUpCount, downactive, value.voteDownCount));
    if (value.innerComments.length > 0) {
        $temp.find("div[id=innerList]").first().css("display", "");
    }
    var innerId;
    $.each(value.innerComments, function (key, value2) {
         upactive = "";
         downactive = "";
        if (value2.currentUserAction == 1) {
            upactive = "VoteUpActive";
        }
        else if (value2.currentUserAction == -1) {
            downactive = "VoteDownActive";
        }
        var $innerTemp = $("#innerComments").clone();
        $innerTemp.css("display", "");
        $innerTemp.find("div[id=userAndText2]").html($.format($("#userAndText2").html(), value2.userName, value2.replyToCommentUserName, value2.text));
        $innerTemp.find("div[id=timeAndVote]").html($.format($("#timeAndVote").html(), value2.time, upactive, value2.id, value2.voteUpCount, downactive, value2.voteDownCount));
        $temp.find("div[id=innerList]").first().append($innerTemp);
        innerId = value2.id;
    })
    $temp.find("button[id=loadInnerBtn]").attr("onclick", $.format("LoadMoreInnerComment({0},{1},this.parentElement,this)", value.id, innerId));
    $temp.find("div[id=innerList]").append($temp.find("button[id=loadInnerBtn]"));
    return $temp;
}
function GetArticleElement(value) {
    var upactive = "";
    var downactive = "";
    if (value.currentUserAction == 1) {
        upactive = "VoteUpActive"
    }
    else if (value.currentUserAction == -1) {
        downactive = "VoteDownActive";
    }
    var $temp = $("#articleViewTemp").clone();
    $temp.html($.format($temp.html(),
        value.title,
        value.createTime,
        value.authorName,
        value.id,
        value.cover,
        value.viewCount,
        upactive,
        value.voteUpNumber,
        downactive,
        value.voteDownNumber
    ))
    return $temp;
}