  "use_strict"
  function opVec2 (f, i, acc, xs, ys) {
    if (i > (xs.length - 1)) { return acc; }
    else {
        acc[i] = f (xs[i], ys[i]);
        return opVec2 (f, (i + 1), acc, xs, ys);
    }
  }
  function mulVec2 (xs, ys) { return opVec2 ((function (x,y) { return x * y; }), 0, [], xs, ys); }
  function add (x, y) { return x + y; }
  function addVec (xs, ys) { return opVec2 (add, 0, [], xs, ys); }
  function wsRec (i, acc, ixs, ws_xss) {
    if (i > (ws_xss.length - 1)) { return acc; }
    acc[i] = (mulVec2 (ixs, ws_xss[i]).reduce(add));
    return wsRec ((i + 1), acc, ixs, ws_xss);
  }
  function ws (inputs, weights, bias) { return addVec (bias, (wsRec (0, [], inputs, weights))); }
  function ff (f, inputs) {
    var ihws = [ 
        [ 3.831233387, 4.01329833, 6.296136811, 6.112385545 ],
        [ -4.343587541, 2.251556646, 6.595788413, -6.756682305 ]];
    var hbs = [ -10.4525315, 8.912132427 ];
    var hows = [
        [ -0.00339809083, -1.34502627 ],
        [ -1.44093948, 1.43240389 ],
        [ 1.307363047, -0.001861278349 ]];
    var obs = [ 1.336714147, -0.01862609591, 1.303547049 ];

    var hWeightedSum = f (inputs, ihws, hbs);
    var hNetOuts = hWeightedSum.map(Math.tanh);
    var oWeightedSum = f (hNetOuts, hows, obs);
    var outputs = oWeightedSum.map(Math.tanh);

    return outputs;
  }
  function predict (w, x, y, z) {
    var inputs = [ w, x, y, z ];
    var outputs = ff (ws, inputs);
    return outputs;
  }
  function displayOutput (cont, value) { cont.innerHTML = value; return cont; }
  function run ()
  {
    var age = (document.getElementById("age_select")).value;
    var spec_persc = (document.getElementById("spec_persc_select")).value;
    var isAstigmatism = (document.getElementById("astigmatism_select")).value;
    var tear_prod_rate = (document.getElementById("tear_prod_rate_select")).value;

    var predicted_value = predict (age, spec_persc, isAstigmatism, tear_prod_rate);

    var cont_xs = [
      document.getElementById("soft_cont"),
      document.getElementById("none_cont"),
      document.getElementById("hard_cont")];

    var class_xs = ["Soft: ", "None: ", "Hard: "];

    var displayClassPercentage = (function f (i, displayf, value_xs, cont_xs, class_xs)
    {
      if (i > (cont_xs.length - 1)) { return; }
      var cent = 100;
      var value = Math.round(cent * value_xs[i]);
      var class_percentage = class_xs[i] + value + "%";
      var display_accuracy = displayf(cont_xs[i], class_percentage);
      return f ((i + 1), displayf, value_xs, cont_xs, class_xs);
    })(0, displayOutput, predicted_value, cont_xs, class_xs);

    return predicted_value;
  }
