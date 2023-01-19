function [tiempo] = ConvTime(array)
    
    if length(array) > 1
        tiempo = array(1)*60 + array(2);
    else 
        tiempo = array;
    end
    
    tiempo = floor(tiempo/0.02);
end

