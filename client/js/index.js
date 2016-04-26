//PUB SUB KEYS
var pub_key = "pub-c-913ab39c-d613-44b3-8622-2e56b8f5ea6d";
var sub_key = "sub-c-8ad89b4e-a95e-11e5-a65d-02ee2ddab7fe";

//Constants
// var MAX_HISTORY_FETCH_LIMIT = 30; //In Minutes
// var MAX_HISTORY_SAMPLE_LIMIT = 60; //Number of samples
var BUTTON_TXT_TREND = 'Trend';
var BUTTON_TXT_COUNTER = 'Counter';

//Counter Display State , to indicate if the current state of display is counter or trend
var counterDisplayState = {
    'EUR'   : true,
    'AUD'   : true,
    'CNY'   : true,
    'INR'   : true
};

var trend_graph = {
    'EUR'   : null,
    'AUD'   : null,
    'CNY'   : null,
    'INR'   : null
};

var prevChange = {
    'EUR'   : 0,
    'AUD'   : 0,
    'CNY'   : 0,
    'INR'   : 0
};

var changeValue;

//YOUR PUBNUB KEYS - Replace the publish_key and subscribe_key below with your own keys
var pubnub = PUBNUB.init({
    publish_key: pub_key,
    subscribe_key: sub_key
});

//OnCLick trigger for Trend/Counter Button
var sendRequest = function(p_name){
    pubnub.publish({
        channel : 'trendRequestChannel',
        message : '{\"name\":\"' + p_name + '\"}'
    });
}

var valueChange = function(p_name,p_changeValue){
    var l_currency_present_differance;
    if(parseFloat(p_changeValue) > parseFloat(prevChange[p_name])){
        var changes = p_changeValue - prevChange[p_name];
        changes = changes.toFixed(2);
        prevChange[p_name] = p_changeValue;
        l_currency_present_differance = "+" + changes.toString(); 
        displayDeltaDirectionArrow(p_name,l_currency_present_differance);
    }
    else if(parseFloat(p_changeValue) < parseFloat(prevChange[p_name])){
        var changes = prevChange[p_name] - p_changeValue;
        changes = changes.toFixed(2);
        prevChange[p_name] = p_changeValue;
        l_currency_present_differance = "-" + changes.toString();
        displayDeltaDirectionArrow(p_name,l_currency_present_differance);
    }
    else{
        displayDeltaDirectionArrow(p_name,"+0");
        prevChange[p_name] = p_changeValue;
    }
}

var updatePrice = function(p_rcvmessage){
    var p_message = JSON.parse(p_rcvmessage);
    if(p_message.requestType == 0){
        if(counterDisplayState[p_message.name] == true){
            $('#' + p_message['name']).html(p_message['value'])
            changeValue = p_message['value']
            valueChange(p_message['name'],changeValue)
            var date = new Date(p_message['time'] * 1000);
            var timeString = date.getUTCHours().toString() + ":" + date.getUTCMinutes().toString() + ":" + date.getUTCSeconds().toString();
            $('#' + p_message['name'] + '-time' ).html(timeString);
        }
        else{
            if(trend_graph[p_message.name] != null){
                prevChange[p_message.name] = trend_graph[p_message.name][trend_graph[p_message.name].length-1]
                valueChange(p_message.name,p_message.value)
                trend_graph[p_message.name].shift();
                trend_graph[p_message.name].push(p_message.value);
                $('#'+p_message.name).sparkline(trend_graph[p_message.name])
                var date = new Date(p_message['time'] * 1000);
                var timeString = date.getUTCHours().toString() + ":" + date.getUTCMinutes().toString() + ":" + date.getUTCSeconds().toString();
                $('#' + p_message['name'] + '-time' ).html(timeString);
                $('[data-index='+p_message.name+']').text(BUTTON_TXT_COUNTER);
            }
            //If there is a connection error with server, waits for the server to connect
            else if($('[data-index='+p_message.name+']').text() == "Loading.."){
                try{
                    sendRequest(p_message.name)
                }
                catch(err){
                    console.log(err)
                }
            }
        }
    }
	else if(p_message.requestType == 1){
        if(counterDisplayState[p_message.name] == false){
            trend_graph[p_message['name']] = p_message['value'];
            $('#'+p_message.name).sparkline(trend_graph[p_message.name])
            $('[data-index='+p_message.name+']').text(BUTTON_TXT_COUNTER);
        }
    }
};

var displayDeltaDirectionArrow = function(p_name,p_delta){
    //Update Delta Direction
    if('+' == p_delta.charAt(0)) {
        if( $('#' + p_name + '-dir' ).hasClass('triangle-down')) {
            $('#' + p_name + '-dir' ).removeClass('triangle-down');
            $('#' + p_name + '-dir' ).addClass('triangle-up');
        }
        if(! $('#' + p_name + '-dir' ).hasClass('triangle-up')) {

            $('#' + p_name + '-dir' ).addClass('triangle-up');
        }
    } else if ('-' == p_delta.charAt(0)) {
        if( $('#' + p_name + '-dir' ).hasClass('triangle-up')) {
            $('#' + p_name + '-dir' ).removeClass('triangle-up');
            $('#' + p_name + '-dir' ).addClass('triangle-down');
        }
        if(! $('#' + p_name + '-dir' ).hasClass('triangle-down')) {
            $('#' + p_name + '-dir' ).addClass('triangle-down');
        }
    }
    //Update Delta Value
    $('#' + p_name + '-delta' ).html(p_delta.substr(1));
};

$(document ).ready(function() {
	pubnub.subscribe({
	    channel: 'exchangedata',
	    message: updatePrice
	});

	$('button').click(function(){
        if($(this).text() == BUTTON_TXT_TREND){
            //If the Button text is 'Trend' , send request to fetch historical values 
            $(this).text('Loading..');
            sendRequest($(this).data('index'));
            counterDisplayState[$(this).data('index')] = false;
        } else if($(this).text() == BUTTON_TXT_COUNTER) {
            //Change the text
            counterDisplayState[$(this).data('index')] = true;
            $(this).text(BUTTON_TXT_TREND);
        }
    });
});