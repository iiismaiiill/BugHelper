/*!
 * jQuery Upvote - a voting plugin
 * ------------------------------------------------------------------
 *
 * jQuery Upvote is a plugin that generates a voting widget like
 * the one used on Stack Exchange sites.
 *
 * Licensed under Creative Commons Attribution 3.0 Unported
 * http://creativecommons.org/licenses/by/3.0/
 *
 * @version         2.0.0
 * @since           2013.06.19
 * @author          Janos Gyerik
 * @homepage        https://janosgyerik.github.io/upvotejs
 * @twitter         twitter.com/janosgyerik
 *
 * ------------------------------------------------------------------
 *
 *  <div id="topic" class="upvote">
 *    <a class="upvote"></a>
 *    <span class="count"></span>
 *    <a class="downvote"></a>
 *    <a class="star"></a>
 *  </div>
 *
 *  $('#topic').upvote();
 *  $('#topic').upvote({count: 5, upvoted: true});
 *
 */

;(function($) {
  "use strict";
  const namespace = 'upvote';
  const dot_namespace = '.' + namespace;
  const enabledClass = 'upvote-enabled';

  function init(dom, options) {
    return dom.each(function(i) {
      const jqdom = $(this);
      methods.destroy(jqdom);

      const id = dom.attr('id');
      const obj = Upvote.create(id, options);

      jqdom.data(namespace, obj);
    });
  }

  function upvote(jqdom) {
    jqdom.data(namespace).upvote();
    return jqdom;
  }

  function downvote(jqdom) {
    jqdom.data(namespace).downvote();
    return jqdom;
  }

  function star(jqdom) {
    jqdom.data(namespace).star();
    return jqdom;
  }

  function count(jqdom) {
    return jqdom.data(namespace).count();
  }

  function upvoted(jqdom) {
    return jqdom.data(namespace).upvoted();
  }

  function downvoted(jqdom) {
    return jqdom.data(namespace).downvoted();
  }

  function starred(jqdom) {
    return jqdom.data(namespace).starred();
  }

  const methods = {
    init: init,
    count: count,
    upvote: upvote,
    upvoted: upvoted,
    downvote: downvote,
    downvoted: downvoted,
    starred: starred,
    star: star,
    destroy: destroy
  };

  function destroy(jqdom) {
    return jqdom.each(function() {
      $(window).unbind(dot_namespace);
      $(this).removeClass(enabledClass);
      $(this).removeData(namespace);
    });
  }

  $.fn.upvote = function(method) {  
    var args;
    if (methods[method]) {
      args = Array.prototype.slice.call(arguments, 1);
      args.unshift(this);
      return methods[method].apply(this, args);
    }
    if (typeof method === 'object' || ! method) {
      args = Array.prototype.slice.call(arguments);
      args.unshift(this);
      return methods.init.apply(this, args);
    }
    $.error('Method ' + method + ' does not exist on jQuery.upvote');
  };  
})(jQuery);
