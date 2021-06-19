
# library to manage data
library(dplyr)

# library for plots
library(ggpubr)

#library for some statistical tests
library(car)


### read data
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'
input_file_pattern = 'nasa_stats.csv'
in_data_nasa <- read.csv( paste(input_path,input_file_pattern, sep ='') , sep = ';' )

# Transform none to na
in_data_nasa <- na_if(in_data_nasa, 'None')

# transform to numeric values
in_data_nasa$type_test <- as.factor(in_data_nasa$type_test)
in_data_nasa[, c(2:10)]  = as.numeric( unlist( in_data_nasa[, c(2:10)] ) )
str(in_data_nasa)

# normality test
# total
hist( in_data_nasa$Total, 20  )
ggqqplot( in_data_nasa$Total  )
# by level
hist( in_data_nasa[ in_data_nasa$level==0, ]$Total )
hist( in_data_nasa[ in_data_nasa$level==1, ]$Total )
hist( in_data_nasa[ in_data_nasa$level==2, ]$Total  )
ggqqplot(  in_data_nasa[ in_data_nasa$level==0, ]$Total )
ggqqplot(  in_data_nasa[ in_data_nasa$level==1, ]$Total )
ggqqplot(  in_data_nasa[ in_data_nasa$level==2, ]$Total )

# equal variance test
# h0: variance are equal.
# i am expecting a big p-value
var.test( in_data_nasa[ in_data_nasa$level==0, ]$Total , in_data_nasa[ in_data_nasa$level==2, ]$Total , alternative = "two.sided")
?var.test

# t-test
t.test( in_data_nasa[ in_data_nasa$level==0, ]$Total , in_data_nasa[ in_data_nasa$level==2, ]$Total  )









