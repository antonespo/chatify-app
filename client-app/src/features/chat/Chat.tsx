import React, { useContext, useEffect } from "react";
import { Segment, Form, Button, Grid } from "semantic-ui-react";
import { RootStoreContext } from "../../app/stores/rootStore";
import { Form as FinalForm, Field } from "react-final-form";
import { observer } from "mobx-react-lite";
import { IMessage } from "../../app/models/message";
// RCE CSS
import "react-chat-elements/dist/main.css";
// MessageBox component
import { MessageBox } from "react-chat-elements";
import TextInput from "./../../app/common/form/TextInput";
import { format } from "date-fns";
import { ThemeProvider, MessageList } from "@livechat/ui-kit";

const Chat = () => {
  const rootStore = useContext(RootStoreContext);
  const { user } = rootStore.UserStore;
  const {
    messages: chat,
    createHubConnection,
    stopHubConnection,
    addMessage,
  } = rootStore.ChatStore;

  useEffect(() => {
    const nestedFunc = async () => {
      await createHubConnection();
    };

    nestedFunc();
    return () => {
      stopHubConnection();
    };
  }, [createHubConnection, stopHubConnection]);

  const messageSide = (message: IMessage) => {
    if (message.userName === user?.username) {
      return "right";
    } else {
      return "left";
    }
  };

  return (
    <Segment attached>
      <ThemeProvider>
        <div style={{ width: "auto", height: 500 }}>
          <MessageList active>
            {chat &&
              chat.map((message) => (
                <MessageBox
                  key={message.id}
                  position={messageSide(message)}
                  type={"text"}
                  text={message.body}
                  title={message.userName}
                  dateString={format(new Date(message.createAt), "k:mm")}
                />
              ))}
          </MessageList>
        </div>
        <br />
        <FinalForm
          onSubmit={addMessage}
          render={({ handleSubmit, submitting, form }) => (
            <Form onSubmit={() => handleSubmit()!.then(() => form.reset())}>
              <Grid columns="two" divided>
                <Grid.Row>
                  <Grid.Column width={14}>
                    <Field
                      name="body"
                      component={TextInput}
                      placeholder="Type message..."
                    />
                  </Grid.Column>
                  <Grid.Column width={2}>
                    <Button
                      // content="Add Message"
                      // labelPosition="left"
                      icon="send"
                      primary
                      loading={submitting}
                    />
                  </Grid.Column>
                </Grid.Row>
              </Grid>
            </Form>
          )}
        />
      </ThemeProvider>
    </Segment>
  );
};

export default observer(Chat);
