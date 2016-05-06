//PUB SUB KEYS
var pub_key = "pub-c-913ab39c-d613-44b3-8622-2e56b8f5ea6d";
var sub_key = "sub-c-8ad89b4e-a95e-11e5-a65d-02ee2ddab7fe";

//Constants
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


//YOUR PUBNUB KEYS - Replace the publish_key and subscribe_key below with your own keys
var pubnub = PUBNUB.init({
    publish_key: pub_key,
    subscribe_key: sub_key
});

//OnCLick trigger for Trend/Counter Button
var sendRequest = function(p_name,p_status){
    pubnub.publish({
        channel : 'appRequestChannel',
        message : '{\"name\":\"' + p_name + '\",\"requestType\":'+ p_status +'}'
    });
}

var valueChange = function(p_name,p_direction,p_magnitude){
        p_delta = p_direction + p_magnitude
        displayDeltaDirectionArrow(p_name,p_delta);
}

var updatePrice = function(p_rcvmessage){
    var p_message = JSON.parse(p_rcvmessage);
    if(p_message.responseType == 0){
        if(counterDisplayState[p_message.name] == true){

            $('#' + p_message['name']).html(parseFloat(p_message['value']).toFixed(2))
            valueChange(p_message['name'],p_message['direction'],parseFloat(p_message['magnitude']).toFixed(5))
            var date = new Date(p_message['time'] * 1000);
            var timeString = date.toLocaleTimeString();
            $('#' + p_message['name'] + '-time' ).html(timeString);
        }
        else{
            if(trend_graph[p_message.name] != null){
                valueChange(p_message.name,p_message['direction'],parseFloat(p_message['magnitude']).toFixed(5))
                trend_graph[p_message.name].shift();
                trend_graph[p_message.name].push(p_message.value);
                $('#'+p_message.name).sparkline(trend_graph[p_message.name])
                var date = new Date(p_message['time'] * 1000);
                var timeString = date.toLocaleTimeString();
                $('#' + p_message['name'] + '-time' ).html(timeString);
                $('[data-index='+p_message.name+']').text(BUTTON_TXT_COUNTER);
            }
        }
    }
	else if(p_message.responseType == 1){
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

    sendRequest("EUR",0);
    sendRequest("AUD",0);
    sendRequest("CNY",0);
    sendRequest("INR",0);

	$('button').click(function(){
        if($(this).text() == BUTTON_TXT_TREND){
            //If the Button text is 'Trend' , send request to fetch historical values 
            $(this).text('Loading..');
            sendRequest($(this).data('index'),1);
            counterDisplayState[$(this).data('index')] = false;
        } else if($(this).text() == BUTTON_TXT_COUNTER) {
            //Change the text
            counterDisplayState[$(this).data('index')] = true;
            $(this).text(BUTTON_TXT_TREND);
            sendRequest($(this).data('index'),0);
        }
    });
});
